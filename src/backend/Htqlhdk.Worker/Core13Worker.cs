using System.Globalization;
using System.Text.Json.Nodes;
using Htqlhdk.Core;
using Htqlhdk.Worker.Automation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Htqlhdk.Worker;

public sealed class Core13Worker(CoreDatabase database, CoreService service, IZapRunner runner, ZapPlanRenderer renderer,
    IOptions<ZapAutomationOptions> settings, ILogger<Core13Worker> logger) : BackgroundService
{
    private readonly ZapAutomationOptions options = settings.Value;
    private static readonly Action<ILogger, bool, Exception?> ReadyLog = LoggerMessage.Define<bool>(LogLevel.Information, new EventId(1301, "CoreReady"), "Core13 worker ready. ZAP dispatch enabled: {Enabled}");
    private static readonly Action<ILogger, string, Exception?> CycleLog = LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1302, "CoreCycle"), "Core13 worker cycle interrupted: {Type}");
    private static readonly Action<ILogger, long, string, Exception?> RunLog = LoggerMessage.Define<long, string>(LogLevel.Warning, new EventId(1303, "CoreRun"), "Run {Id} failed: {Type}");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (logger.IsEnabled(LogLevel.Information)) { ReadyLog(logger, options.Enabled, null); }
        if (!options.Enabled) { await Task.Delay(Timeout.Infinite, stoppingToken); return; }
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RecoverAsync(stoppingToken);
                await ScheduleAsync(stoppingToken);
                Dispatch? dispatch = await ClaimAsync(stoppingToken);
                if (dispatch is not null) { await ExecuteRunAsync(dispatch, stoppingToken); continue; }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception exception) when (exception is NpgsqlException or BusinessException or IOException or InvalidOperationException or ArgumentException or TimeZoneNotFoundException)
            {
                // Log only exception type: database/runner messages may contain target data or credentials.
                CycleLog(logger, exception.GetType().Name, null);
            }
            await Task.Delay(TimeSpan.FromSeconds(options.PollIntervalSeconds), stoppingToken);
        }
    }

    public async Task ScheduleAsync(CancellationToken ct)
    {
        await using DbSession db = await database.OpenAsync(ct);
        List<JsonObject> plans = await db.QueryAsync("SELECT p.* FROM scan_plan p WHERE status='Active' ORDER BY id FOR UPDATE SKIP LOCKED", ct);
        foreach (JsonObject plan in plans)
        {
            List<JsonObject> existing = await db.QueryAsync("SELECT max(scheduled_for) AS last FROM scan_run WHERE plan_id=@id", ct, ("id", plan.Number("id")));
            DateTime? last = existing[0].NullableDate("last");
            DateTime? due = plan.Text("planKind") == "Verification" ? last is null ? plan.Date("requestedFor") : null :
                Recurrence.Next(plan.Text("recurrenceRule"), plan.Text("timezone"), plan.Date("effectiveFrom"), last ?? plan.Date("effectiveFrom").AddTicks(-1), plan.NullableDate("effectiveUntil"));
            if (due is null)
            {
                await db.ExecuteAsync("UPDATE scan_plan SET status='Expired' WHERE id=@id AND plan_kind='Periodic'", ct, ("id", plan.Number("id")));
                continue;
            }
            if (due > DateTime.UtcNow) { continue; }
            // One durable run per RRULE occurrence. Unique(plan_id, scheduled_for) also protects multiple workers.
            JsonObject run = await db.OneAsync("INSERT INTO scan_run(plan_id,code,scheduled_for,scan_status) VALUES(@plan,@code,@due,'Queued') ON CONFLICT(plan_id,scheduled_for) DO UPDATE SET code=scan_run.code RETURNING id", ct,
                ("plan", plan.Number("id")), ("code", "RUN-" + Guid.NewGuid().ToString("N")[..16]), ("due", due));
            await Access.AuditAsync(db, null, "worker.schedule", "runs", run.Number("id").ToString(CultureInfo.InvariantCulture), Guid.NewGuid(), ct, reason: "Kỳ quét đến hạn");
        }
        await db.CommitAsync(ct);
    }

    private async Task<Dispatch?> ClaimAsync(CancellationToken ct)
    {
        await using DbSession db = await database.OpenAsync(ct);
        List<JsonObject> queued = await db.QueryAsync("SELECT r.* FROM scan_run r WHERE scan_status='Queued' AND scheduled_for<=now() ORDER BY scheduled_for,id LIMIT 1", ct);
        if (queued.Count == 0) { return null; }
        JsonObject run = queued[0];
        if (!await LockAssetPlanAsync(db, run.Number("planId"), ct, true)) { return null; }
        queued = await db.QueryAsync("SELECT * FROM scan_run WHERE id=@id AND scan_status='Queued' FOR UPDATE SKIP LOCKED", ct, ("id", run.Number("id")));
        if (queued.Count == 0) { return null; }
        run = queued[0];
        JsonObject plan = await db.OneAsync("SELECT * FROM scan_plan WHERE id=@id FOR SHARE", ct, ("id", run.Number("planId")));
        try
        {
            Fields.Require(plan.Text("status") == "Active" && run.NullableDate("cancelRequestedAt") is null, "Kế hoạch bị dừng.");
            Fields.Require(DateTime.UtcNow - run.Date("scheduledFor") <= TimeSpan.FromMinutes(5), "Kỳ quét đã bỏ lỡ; không quét bù ngoài lịch.");
            await CoreService.ValidatePlanAsync(db, plan, DateTime.UtcNow, ct);
            JsonObject scope = await db.OneAsync("SELECT asset_id FROM asset_scope_revision WHERE id=@id", ct, ("id", plan.Number("scopeId")));
            JsonObject asset = await db.OneAsync("SELECT base_url FROM asset WHERE id=@id", ct, ("id", scope.Number("assetId")));
            JsonObject profile = await db.OneAsync("SELECT * FROM zap_scan_profile WHERE id=@id FOR SHARE", ct, ("id", plan.Number("zapProfileId")));
            Actor actor = await Actor.LoadAsync(db, profile.Number("serviceUserId"), ct, true);
            List<ScopeRule> rules = await CoreService.RulesAsync(db, plan.Number("scopeId"), ct);
            int maximum = checked((int)profile.Number("maxDurationMinutes"));
            int stage = Math.Max(1, maximum / (profile.Flag("activeScanEnabled") ? 5 : 3));
            ZapJobSpecification job = new(run.Number("id"), run.Text("code"), plan.Number("id"), scope.Number("assetId"), actor.Id,
                [asset.Text("baseUrl")], rules.Select(r => new ZapScopeRule(r.Effect, r.TargetType, r.TargetValue, r.MatchMode)).ToArray(),
                Enum.Parse<ZapSpiderKind>(profile.Text("spiderKind")), profile.Flag("activeScanEnabled"), Enum.Parse<ZapAuthenticationKind>(profile.Text("authKind")),
                profile.Optional("authHeaderName"), profile.Optional("authSecretRef"), stage, stage, stage, Math.Max(1, stage / 2),
                checked((int)profile.Number("requestDelayMs")), checked((int)profile.Number("threadPerHost")), DateTimeOffset.UtcNow, maximum);
            await db.ExecuteAsync("UPDATE scan_run SET scan_status='Running',started_at=now(),heartbeat_at=now(),runner_name='core13-docker',image_reference=@image WHERE id=@id", ct,
                ("id", job.ExecutionId), ("image", options.ImageReference));
            await Access.AuditAsync(db, actor, "worker.start", "runs", job.ExecutionId.ToString(CultureInfo.InvariantCulture), Guid.NewGuid(), ct);
            await db.CommitAsync(ct);
            return new(job, actor, profile.Text("reportFormat"), plan);
        }
        catch (BusinessException)
        {
            await db.ExecuteAsync("UPDATE scan_run SET scan_status=CASE WHEN cancel_requested_at IS NOT NULL THEN 'Cancelled' ELSE 'Failed' END,finished_at=now(),failure_code='PRECONDITION_FAILED',failure_message_masked='Không đủ điều kiện thực thi trong lịch và phạm vi đã duyệt.' WHERE id=@id", ct, ("id", run.Number("id")));
            await Access.AuditAsync(db, null, "worker.precondition", "runs", run.Number("id").ToString(CultureInfo.InvariantCulture), Guid.NewGuid(), ct, "Failure");
            await db.CommitAsync(ct);
            return null;
        }
    }

    private async Task ExecuteRunAsync(Dispatch dispatch, CancellationToken hostToken)
    {
        using CancellationTokenSource deadline = new(TimeSpan.FromMinutes(dispatch.Job.PlannedDurationMinutes));
        using CancellationTokenSource runToken = CancellationTokenSource.CreateLinkedTokenSource(hostToken, deadline.Token);
        Task watch = WatchAsync(dispatch, runToken);
        try
        {
            ZapRunResult result = await runner.RunAsync(dispatch.Job, renderer.Render(dispatch.Job, dispatch.Format), runToken.Token);
            string status = result.TimedOut ? "Timed Out" : result.ExitCode is 0 or 2 ? "Completed" : "Failed";
            await FinishAsync(dispatch, status, result.ExitCode, hostToken);
            if (status == "Completed") { await ReceiveAsync(dispatch, hostToken); }
        }
        catch (OperationCanceledException)
        {
            await FinishAsync(dispatch, deadline.IsCancellationRequested ? "Timed Out" : "Cancelled", null, CancellationToken.None);
        }
        catch (Exception exception) when (exception is IOException or InvalidOperationException or System.ComponentModel.Win32Exception or NpgsqlException or BusinessException)
        {
            await FinishAsync(dispatch, "Failed", null, CancellationToken.None);
            RunLog(logger, dispatch.Job.ExecutionId, exception.GetType().Name, null);
        }
        finally
        {
            await runToken.CancelAsync();
            try { await watch; } catch (OperationCanceledException) { }
            if (options.CleanupWorkDirectory)
            {
                string root = Path.GetFullPath(options.WorkRoot);
                string path = Path.GetFullPath(Path.Combine(root, dispatch.Job.ExecutionCode));
                if (path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) && Directory.Exists(path)) { Directory.Delete(path, true); }
            }
        }
    }

    private async Task WatchAsync(Dispatch dispatch, CancellationTokenSource runToken)
    {
        while (!runToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(options.HeartbeatSeconds), runToken.Token);
            try
            {
                await using DbSession db = await database.OpenAsync(runToken.Token);
                JsonObject run = await db.OneAsync("SELECT cancel_requested_at FROM scan_run WHERE id=@id", runToken.Token, ("id", dispatch.Job.ExecutionId));
                JsonObject plan = await db.OneAsync("SELECT * FROM scan_plan WHERE id=@id", runToken.Token, ("id", dispatch.Job.ScanId));
                Fields.Require(run.NullableDate("cancelRequestedAt") is null && plan.Text("status") == "Active", "Đã yêu cầu dừng.");
                await CoreService.ValidatePlanAsync(db, plan, DateTime.UtcNow, runToken.Token, dispatch.Job.StartedAt.UtcDateTime.AddMinutes(dispatch.Job.PlannedDurationMinutes));
                await db.ExecuteAsync("UPDATE scan_run SET heartbeat_at=now() WHERE id=@id AND scan_status='Running'", runToken.Token, ("id", dispatch.Job.ExecutionId));
                await db.CommitAsync(runToken.Token);
            }
            catch (Exception exception) when (exception is BusinessException or NpgsqlException)
            {
                await runToken.CancelAsync(); return;
            }
        }
    }

    private async Task FinishAsync(Dispatch dispatch, string status, int? exitCode, CancellationToken ct)
    {
        await using DbSession db = await database.OpenAsync(ct);
        await LockAssetPlanAsync(db, dispatch.Job.ScanId, ct);
        await db.ExecuteAsync("UPDATE scan_run SET scan_status=@status,finished_at=now(),exit_code=@exit,failure_code=CASE WHEN @status<>'Completed' THEN 'RUNNER_STOPPED' END,failure_message_masked=CASE WHEN @status<>'Completed' THEN 'Lần quét dừng hoặc không hoàn thành.' END WHERE id=@id AND scan_status='Running'", ct,
            ("id", dispatch.Job.ExecutionId), ("status", status), ("exit", exitCode));
        await Access.AuditAsync(db, dispatch.Actor, "worker.finish", "runs", dispatch.Job.ExecutionId.ToString(CultureInfo.InvariantCulture), Guid.NewGuid(), ct, status == "Completed" ? "Success" : "Failure", after: new JsonObject { ["scanStatus"] = status });
        await db.CommitAsync(ct);
    }

    private async Task ReceiveAsync(Dispatch dispatch, CancellationToken ct)
    {
        try
        {
            string path = Path.Combine(Path.GetFullPath(options.WorkRoot), dispatch.Job.ExecutionCode, "output", "report." + dispatch.Format.ToLowerInvariant());
            Fields.Require(File.Exists(path) && new FileInfo(path).Length <= options.MaxReportBytes, "Báo cáo thiếu hoặc vượt giới hạn.");
            byte[] bytes = await File.ReadAllBytesAsync(path, ct);
            await using DbSession db = await database.OpenAsync(ct);
            Actor actor = await Actor.LoadAsync(db, dispatch.Actor.Id, ct, true);
            JsonObject run = await db.OneAsync("SELECT xmin::text::bigint AS version FROM scan_run WHERE id=@id", ct, ("id", dispatch.Job.ExecutionId));
            await service.ImportAsync(db, actor, dispatch.Job.ExecutionId, bytes, dispatch.Format, "\"" + run.Number("version").ToString(CultureInfo.InvariantCulture) + "\"", ct);
            await Access.AuditAsync(db, actor, "worker.report", "runs", dispatch.Job.ExecutionId.ToString(CultureInfo.InvariantCulture), Guid.NewGuid(), ct);
            await db.CommitAsync(ct);
        }
        catch (Exception exception) when (exception is BusinessException or System.Text.Json.JsonException or System.Xml.XmlException or IOException or NpgsqlException)
        {
            await using DbSession db = await database.OpenAsync(ct);
            await CoreService.RejectReportAsync(db, dispatch.Actor, dispatch.Job.ExecutionId, "REPORT_INVALID", ct);
            await Access.AuditAsync(db, dispatch.Actor, "worker.report", "runs", dispatch.Job.ExecutionId.ToString(CultureInfo.InvariantCulture), Guid.NewGuid(), ct, "Failure");
            await db.CommitAsync(ct);
        }
    }

    private async Task RecoverAsync(CancellationToken ct)
    {
        await using DbSession db = await database.OpenAsync(ct);
        List<JsonObject> lost = await db.QueryAsync("SELECT id,code,plan_id FROM scan_run WHERE scan_status='Running' AND heartbeat_at < now()-make_interval(secs=>@lease)", ct, ("lease", options.LeaseSeconds));
        foreach (JsonObject run in lost)
        {
            if (!await LockAssetPlanAsync(db, run.Number("planId"), ct, true)) { continue; }
            if ((await db.QueryAsync("SELECT id FROM scan_run WHERE id=@id AND scan_status='Running' AND heartbeat_at < now()-make_interval(secs=>@lease) FOR UPDATE SKIP LOCKED", ct, ("id", run.Number("id")), ("lease", options.LeaseSeconds))).Count == 0) { continue; }
            if (runner is DockerZapRunner docker) { await docker.StopExecutionAsync(run.Text("code")); }
            await db.ExecuteAsync("UPDATE scan_run SET scan_status='Failed',finished_at=now(),failure_code='WORKER_LOST',failure_message_masked='Worker mất kết nối; cần tạo kỳ quét mới.' WHERE id=@id", ct, ("id", run.Number("id")));
            await Access.AuditAsync(db, null, "worker.recover", "runs", run.Number("id").ToString(CultureInfo.InvariantCulture), Guid.NewGuid(), ct, "Failure");
        }
        await db.CommitAsync(ct);
    }

    private static async Task<bool> LockAssetPlanAsync(DbSession db, long id, CancellationToken ct, bool skip = false)
    {
        string suffix = skip ? " SKIP LOCKED" : "";
        if ((await db.QueryAsync("SELECT a.id FROM asset a JOIN asset_scope_revision s ON s.asset_id=a.id JOIN scan_plan p ON p.scope_id=s.id WHERE p.id=@id FOR UPDATE OF a" + suffix, ct, ("id", id))).Count == 0) { return false; }
        return (await db.QueryAsync("SELECT id FROM scan_plan WHERE id=@id FOR UPDATE" + suffix, ct, ("id", id))).Count > 0;
    }

    private sealed record Dispatch(ZapJobSpecification Job, Actor Actor, string Format, JsonObject Plan);
}
