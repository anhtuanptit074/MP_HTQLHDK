using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using Htqlhdk.Core;
using Htqlhdk.Worker.Automation;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Htqlhdk.Worker.Tests;

[TestClass]
[TestCategory("DatabaseIntegration")]
public sealed class CoreWorkerIntegrationTests
{
    [TestMethod]
    public async Task SchedulerClaimsOnceImportsXmlAndRecoversCancelledOrLostRuns()
    {
        string? connection = Environment.GetEnvironmentVariable("HTQLHDK_TEST_DATABASE_URL");
        if (connection is null) { Assert.Inconclusive("Run scripts/test-core13.ps1 to exercise the isolated PostgreSQL worker integration test."); }
        NpgsqlConnectionStringBuilder check = new(connection!);
        Assert.AreEqual("htqlhdk_core_test_local", check.Database, "Worker integration must never run against the user database.");
        await using NpgsqlDataSource source = NpgsqlDataSource.Create(connection!);
        CoreDatabase database = new(source);
        string root = Environment.GetEnvironmentVariable("HTQLHDK_TEST_ROOT")!;
        Protection protection = CoreConfiguration.Storage(true);
        string workRoot = Path.Combine(root, "worker-work");
        ZapAutomationOptions options = new() { Enabled = true, WorkRoot = workRoot, PollIntervalSeconds = 1, HeartbeatSeconds = 5, LeaseSeconds = 30 };
        SimulatedRunner runner = new(workRoot);
        using Core13Worker worker = new(database, new CoreService(protection), runner, new ZapPlanRenderer(), Options.Create(options), NullLogger<Core13Worker>.Instance);
        long planId;
        long cancelledId;
        long lostId;
        long completedId;
        await using (DbSession db = await database.OpenAsync())
        {
            JsonObject fixture = await db.OneAsync("SELECT s.id AS scope_id,p.service_user_id,p.created_by,p.approved_by,sp.operator_id FROM asset_scope_revision s JOIN asset a ON a.id=s.asset_id JOIN scan_plan sp ON sp.scope_id=s.id JOIN zap_scan_profile p ON p.id=sp.zap_profile_id WHERE a.code='SHOP-DEMO' LIMIT 1", default);
            JsonObject profile = await db.OneAsync("INSERT INTO zap_scan_profile(code,name,version_no,status,report_format,service_user_id,created_by,approved_by,approved_at,max_duration_minutes) VALUES(@code,'Worker XML test',1,'Active','XML',@service,@creator,@approver,now(),1) RETURNING id", default,
                ("code", "WXML-" + Guid.NewGuid().ToString("N")[..12]), ("service", fixture.Number("serviceUserId")), ("creator", fixture.Number("createdBy")), ("approver", fixture.Number("approvedBy")));
            JsonObject plan = await db.OneAsync("INSERT INTO scan_plan(code,name,plan_kind,scope_id,zap_profile_id,operator_id,status,recurrence_rule,timezone,effective_from,created_by,approved_by,approved_at) VALUES(@code,'Worker integration','Periodic',@scope,@profile,@operator,'Active','FREQ=DAILY','Asia/Bangkok',@when,@creator,@approver,now()) RETURNING id", default,
                ("code", "WPLAN-" + Guid.NewGuid().ToString("N")[..12]), ("scope", fixture.Number("scopeId")), ("profile", profile.Number("id")), ("operator", fixture.Number("operatorId")), ("when", DateTime.UtcNow.AddSeconds(-1)), ("creator", fixture.Number("createdBy")), ("approver", fixture.Number("approvedBy")));
            planId = plan.Number("id");
            JsonObject cancel = await db.OneAsync("INSERT INTO scan_run(plan_id,code,scheduled_for,scan_status,cancel_requested_at) VALUES(@plan,@code,now()-interval '2 days','Queued',now()) RETURNING id", default,
                ("plan", planId), ("code", "WCANCEL-" + Guid.NewGuid().ToString("N")[..12]));
            cancelledId = cancel.Number("id");
            JsonObject lost = await db.OneAsync("INSERT INTO scan_run(plan_id,code,scheduled_for,scan_status,started_at,heartbeat_at) VALUES(@plan,@code,now()-interval '1 day','Running',now()-interval '10 minutes',now()-interval '5 minutes') RETURNING id", default,
                ("plan", planId), ("code", "WLOST-" + Guid.NewGuid().ToString("N")[..12]));
            lostId = lost.Number("id");
            await db.CommitAsync();
        }
        await worker.ScheduleAsync(default);
        await worker.ScheduleAsync(default);
        await using (DbSession db = await database.OpenAsync())
        {
            List<JsonObject> runs = await db.QueryAsync("SELECT id FROM scan_run WHERE plan_id=@id AND scan_status='Queued' AND cancel_requested_at IS NULL", default, ("id", planId));
            Assert.HasCount(1, runs);
            completedId = runs[0].Number("id");
        }
        await worker.StartAsync(default);
        bool received = false;
        for (int attempt = 0; attempt < 60; attempt++)
        {
            await using DbSession db = await database.OpenAsync();
            JsonObject row = await db.OneAsync("SELECT scan_status,report_status FROM scan_run WHERE id=@id", default, ("id", completedId));
            if (row.Text("reportStatus") == "Processed") { received = true; break; }
            await Task.Delay(250);
        }
        await worker.StopAsync(default);
        Assert.IsTrue(received, "The worker should finish and atomically import its XML report.");
        await using (DbSession db = await database.OpenAsync())
        {
            Assert.AreEqual("Cancelled", (await db.OneAsync("SELECT scan_status FROM scan_run WHERE id=@id", default, ("id", cancelledId))).Text("scanStatus"));
            Assert.AreEqual("Failed", (await db.OneAsync("SELECT scan_status FROM scan_run WHERE id=@id", default, ("id", lostId))).Text("scanStatus"));
            JsonObject report = await db.OneAsync("SELECT report_format,report_object_key FROM scan_run WHERE id=@id", default, ("id", completedId));
            Assert.AreEqual("XML", report.Text("reportFormat"));
            string decoded = Encoding.UTF8.GetString(await protection.ReadAsync(report.Text("reportObjectKey"), default));
            StringAssert.Contains(decoded, "OWASPZAPReport");
            Assert.AreEqual(1, runner.Executions.Count(id => id == completedId));
        }
    }

    private sealed class SimulatedRunner(string root) : IZapRunner
    {
        internal List<long> Executions { get; } = [];
        public async Task<ZapRunResult> RunAsync(ZapJobSpecification job, string planYaml, CancellationToken cancellationToken)
        {
            StringAssert.Contains(planYaml, "traditional-xml");
            Assert.IsFalse(job.ActiveScanEnabled);
            Executions.Add(job.ExecutionId);
            string path = Path.Combine(root, job.ExecutionCode, "output"); Directory.CreateDirectory(path);
            string report = Path.Combine(path, "report.xml");
            await File.WriteAllTextAsync(report, "<OWASPZAPReport version=\"2.17.0\"><site name=\"" + job.TargetUrls[0] + "\"><alerts /></site></OWASPZAPReport>", cancellationToken);
            return new(0, false, report, Path.Combine(path, "report.html"));
        }
    }
}
