using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

namespace Htqlhdk.Core;

public sealed partial class CoreService
{
    private static async Task<JsonNode> ProfilesAsync(DbSession db, Actor actor, CancellationToken ct)
    {
        Access.Demand(actor.Eligible("SECURITY") || actor.Eligible("SCAN_OPERATOR"));
        return Page(await db.QueryAsync("SELECT " + ProfileColumns + " FROM zap_scan_profile z ORDER BY code,version_no DESC", ct));
    }
    private static async Task<JsonNode> SaveProfileAsync(DbSession db, Actor actor, long? id, JsonObject input, string? etag, CancellationToken ct)
    {
        Access.Demand(actor.Eligible("SCAN_OPERATOR"));
        JsonObject service = await db.OneAsync("SELECT account_kind FROM app_user WHERE id=@id AND is_active", ct, ("id", input.Number("serviceUserId")));
        Fields.Require(service.Text("accountKind") == "Service", "Hồ sơ ZAP phải dùng tài khoản dịch vụ.");
        string auth = input.Text("authKind", "None");
        string? header = auth == "Header" ? input.Text("authHeaderName") : null;
        string? secret = auth == "Header" ? input.Text("authSecretRef") : null;
        Fields.Require(input.Flag("passiveScanEnabled", true), "Hồ sơ phải bật passive scan.");
        if (id is null)
        {
            return await db.OneAsync("INSERT INTO zap_scan_profile(code,name,version_no,spider_kind,active_scan_enabled,max_duration_minutes,request_delay_ms,thread_per_host,auth_kind,auth_header_name,auth_secret_ref,report_format,service_user_id,created_by) SELECT @code,@name,coalesce(max(version_no),0)+1,@spider,@active,@duration,@delay,@threads,@auth,@header,@secret,@format,@service,@actor FROM zap_scan_profile WHERE code=@code RETURNING id,code,version_no,status,xmin::text::bigint AS version", ct,
                ("code", input.Text("code")), ("name", input.Text("name")), ("spider", input.Text("spiderKind", "Traditional")), ("active", input.Flag("activeScanEnabled")),
                ("duration", input.NullableNumber("maxDurationMinutes") ?? 30), ("delay", input.NullableNumber("requestDelayMs") ?? 250), ("threads", input.NullableNumber("threadPerHost") ?? 2),
                ("auth", auth), ("header", header), ("secret", secret), ("format", input.Text("reportFormat", "JSON")), ("service", input.Number("serviceUserId")), ("actor", actor.Id));
        }
        JsonObject profile = await db.OneAsync("SELECT " + ProfileColumns + " FROM zap_scan_profile z WHERE id=@id FOR UPDATE", ct, ("id", id));
        Version(profile, etag); Access.Demand(profile.Number("createdBy") == actor.Id);
        Fields.Require(profile.Text("status") == "Draft", "Chỉ chỉnh sửa hồ sơ nháp.", 409, "STATE");
        return await db.OneAsync("UPDATE zap_scan_profile z SET name=@name,spider_kind=@spider,active_scan_enabled=@active,max_duration_minutes=@duration,request_delay_ms=@delay,thread_per_host=@threads,auth_kind=@auth,auth_header_name=@header,auth_secret_ref=@secret,report_format=@format,service_user_id=@service WHERE id=@id RETURNING " + ProfileColumns, ct,
            ("id", id), ("name", input.Text("name")), ("spider", input.Text("spiderKind", "Traditional")), ("active", input.Flag("activeScanEnabled")),
            ("duration", input.NullableNumber("maxDurationMinutes") ?? 30), ("delay", input.NullableNumber("requestDelayMs") ?? 250), ("threads", input.NullableNumber("threadPerHost") ?? 2),
            ("auth", auth), ("header", header), ("secret", secret), ("format", input.Text("reportFormat", "JSON")), ("service", input.Number("serviceUserId")));
    }
    private static async Task<JsonNode> ProfileActionAsync(DbSession db, Actor actor, long id, string action, JsonObject input, string? etag, CancellationToken ct)
    {
        JsonObject profile = await db.OneAsync("SELECT " + ProfileColumns + " FROM zap_scan_profile z WHERE id=@id FOR UPDATE", ct, ("id", id));
        Version(profile, etag);
        string to;
        if (action == "submit") { Access.Demand(actor.Eligible("SCAN_OPERATOR") && profile.Number("createdBy") == actor.Id); Fields.Require(profile.Text("status") == "Draft", "Hồ sơ không phải nháp."); to = "Pending Approval"; }
        else if (action == "decision")
        {
            Access.Demand(actor.Eligible("SECURITY")); Fields.Require(profile.Text("status") == "Pending Approval", "Hồ sơ chưa gửi duyệt.");
            Fields.Require(profile.Number("createdBy") != actor.Id, "Người tạo không tự phê duyệt hồ sơ ZAP.");
            _ = input.Text("reason"); string decision = input.Text("decision"); Fields.Require(decision is "Approved" or "Rejected", "Quyết định không hợp lệ."); to = decision == "Approved" ? "Active" : "Draft";
            if (to == "Active" && profile.Flag("activeScanEnabled")) { Fields.Require(input.Flag("activeScanApproved"), "Cần xác nhận cho phép active scan riêng."); }
        }
        else if (action == "retire") { Access.Demand(actor.Eligible("SECURITY")); _ = input.Text("reason"); Fields.Require(profile.Text("status") == "Active", "Hồ sơ chưa Active."); to = "Retired"; }
        else { throw new BusinessException(404, "NOT_FOUND", "Thao tác không tồn tại."); }
        return await db.OneAsync("UPDATE zap_scan_profile z SET status=@status,approved_by=CASE WHEN @status='Active' THEN @actor ELSE approved_by END,approved_at=CASE WHEN @status='Active' THEN now() ELSE approved_at END,retired_at=CASE WHEN @status='Retired' THEN now() ELSE NULL END WHERE id=@id RETURNING " + ProfileColumns, ct,
            ("id", id), ("status", to), ("actor", actor.Id));
    }
    private static async Task<JsonNode> PlansAsync(DbSession db, Actor actor, long after, int limit, CancellationToken ct)
    {
        List<JsonObject> plans = await db.QueryAsync($"SELECT {PlanColumns},s.asset_id,a.name AS asset_name FROM scan_plan p JOIN asset_scope_revision s ON s.id=p.scope_id JOIN asset a ON a.id=s.asset_id WHERE {AllowedPlan} AND p.id>@after ORDER BY p.id LIMIT @limit", ct, ("actor", actor.Id), ("after", after), ("limit", limit));
        foreach (JsonObject plan in plans)
        {
            if (plan.Text("planKind") == "Periodic")
            {
                plan["nextDueAt"] = Recurrence.Next(plan.Text("recurrenceRule"), plan.Text("timezone"), plan.Date("effectiveFrom"), DateTime.UtcNow, plan.NullableDate("effectiveUntil"));
            }
            else { plan["nextDueAt"] = plan["requestedFor"]?.DeepClone(); }
        }
        return Page(plans);
    }
    private static async Task<JsonNode> SavePlanAsync(DbSession db, Actor actor, long? id, JsonObject input, string? etag, CancellationToken ct)
    {
        JsonObject scope = await db.OneAsync("SELECT asset_id,status FROM asset_scope_revision WHERE id=@id", ct, ("id", input.Number("scopeId")));
        await RequireAssetAsync(db, actor, scope.Number("assetId"), ct, true);
        Fields.Require(scope.Text("status") == "Approved", "Kế hoạch phải dùng phạm vi đã duyệt.");
        await RequireEligibleAsync(db, input.Number("operatorId"), "SCAN_OPERATOR", scope.Number("assetId"), ct);
        string kind = input.Text("planKind", "Periodic");
        Fields.Require(kind == "Periodic", "Yêu cầu xác minh phải tạo từ phiếu khắc phục.");
        DateTime from = input.Date("effectiveFrom"); DateTime? until = input.NullableDate("effectiveUntil");
        _ = Recurrence.Next(input.Text("recurrenceRule"), input.Text("timezone"), from, from.AddTicks(-1), until);
        var parameters = new (string, object?)[] { ("code", input.Text("code")), ("name", input.Text("name")), ("scope", input.Number("scopeId")), ("profile", input.Number("zapProfileId")),
            ("operator", input.Number("operatorId")), ("rule", input.Text("recurrenceRule")), ("timezone", input.Text("timezone")), ("from", from), ("until", until), ("actor", actor.Id), ("id", id) };
        if (id is null)
        {
            return await db.OneAsync("INSERT INTO scan_plan(code,version_no,name,plan_kind,scope_id,zap_profile_id,operator_id,recurrence_rule,timezone,effective_from,effective_until,created_by) SELECT @code,coalesce(max(version_no),0)+1,@name,'Periodic',@scope,@profile,@operator,@rule,@timezone,@from,@until,@actor FROM scan_plan WHERE code=@code RETURNING id,code,version_no,status,xmin::text::bigint AS version", ct, parameters);
        }
        JsonObject plan = await db.OneAsync("SELECT " + PlanColumns + " FROM scan_plan p WHERE id=@id FOR UPDATE", ct, ("id", id));
        JsonObject oldScope = await db.OneAsync("SELECT asset_id FROM asset_scope_revision WHERE id=@id", ct, ("id", plan.Number("scopeId")));
        Fields.Require(oldScope.Number("assetId") == scope.Number("assetId"), "Không chuyển kế hoạch sang tài sản khác.");
        Version(plan, etag); Fields.Require(plan.Text("status") is "Draft" or "Paused", "Kế hoạch phải Draft/Paused để chỉnh sửa.");
        return await db.OneAsync("UPDATE scan_plan p SET name=@name,scope_id=@scope,zap_profile_id=@profile,operator_id=@operator,recurrence_rule=@rule,timezone=@timezone,effective_from=@from,effective_until=@until WHERE id=@id RETURNING " + PlanColumns, ct, parameters);
    }
    public static async Task ValidatePlanAsync(DbSession db, JsonObject plan, DateTime when, CancellationToken ct, DateTime? deadline = null)
    {
        JsonObject scope = await db.OneAsync("SELECT status,asset_id,effective_from,effective_until FROM asset_scope_revision WHERE id=@id", ct, ("id", plan.Number("scopeId")));
        JsonObject asset = await db.OneAsync("SELECT status,base_url FROM asset WHERE id=@id FOR SHARE", ct, ("id", scope.Number("assetId")));
        JsonObject profile = await db.OneAsync("SELECT status,max_duration_minutes,service_user_id FROM zap_scan_profile WHERE id=@id FOR SHARE", ct, ("id", plan.Number("zapProfileId")));
        Fields.Require(asset.Text("status") == "Active" && scope.Text("status") == "Approved" && scope.Date("effectiveFrom") <= when && (scope.NullableDate("effectiveUntil") is not DateTime end || when < end), "Tài sản hoặc phạm vi không còn hiệu lực.");
        Fields.Require(profile.Text("status") == "Active", "Hồ sơ ZAP chưa được phê duyệt.");
        await RequireEligibleAsync(db, plan.Number("operatorId"), "SCAN_OPERATOR", scope.Number("assetId"), ct);
        JsonObject service = await db.OneAsync("SELECT account_kind,is_active FROM app_user WHERE id=@id", ct, ("id", profile.Number("serviceUserId")));
        Fields.Require(service.Flag("isActive") && service.Text("accountKind") == "Service", "Tài khoản dịch vụ không còn hiệu lực.");
        Fields.Require(ScopeRules.Contains(await RulesAsync(db, plan.Number("scopeId"), ct), asset.Text("baseUrl")), "URL tài sản ngoài phạm vi.");
        DateTime? windowEnd = ScopeRules.WindowEnd(await WindowsAsync(db, plan.Number("scopeId"), ct), when);
        DateTime finishBy = deadline ?? when.AddMinutes(profile.Number("maxDurationMinutes"));
        if (plan.Optional("planKind") == "Periodic")
        {
            Fields.Require(when >= plan.Date("effectiveFrom") && (plan.NullableDate("effectiveUntil") is not DateTime planUntil || finishBy <= planUntil), "Lần quét vượt thời gian hiệu lực kế hoạch.");
        }
        Fields.Require(windowEnd is not null && finishBy <= windowEnd && (scope.NullableDate("effectiveUntil") is not DateTime scopeUntil || finishBy <= scopeUntil), "Lần quét không nằm trọn trong khung giờ và hiệu lực phạm vi được duyệt.");
    }
    private static async Task<JsonNode> PlanActionAsync(DbSession db, Actor actor, long id, JsonObject input, string? etag, CancellationToken ct)
    {
        JsonObject reference = await db.OneAsync("SELECT s.asset_id FROM scan_plan p JOIN asset_scope_revision s ON s.id=p.scope_id WHERE p.id=@id", ct, ("id", id));
        await RequireAssetAsync(db, actor, reference.Number("assetId"), ct, true);
        JsonObject plan = await db.OneAsync("SELECT " + PlanColumns + " FROM scan_plan p WHERE id=@id FOR UPDATE", ct, ("id", id)); Version(plan, etag);
        string to = input.Text("toStatus"); _ = input.Text("reason");
        Fields.Require((plan.Text("status"), to) is ("Draft" or "Paused", "Active") or ("Active", "Paused" or "Expired" or "Cancelled") or ("Draft" or "Paused", "Cancelled"), "Chuyển trạng thái kế hoạch không hợp lệ.", 409, "STATE");
        if (to == "Active")
        {
            DateTime when = plan.Text("planKind") == "Verification" ? plan.Date("requestedFor") :
                Recurrence.Next(plan.Text("recurrenceRule"), plan.Text("timezone"), plan.Date("effectiveFrom"), DateTime.UtcNow, plan.NullableDate("effectiveUntil")) ?? throw new BusinessException(422, "SCHEDULE", "Lịch không còn kỳ quét hợp lệ.");
            await ValidatePlanAsync(db, plan, when, ct);
        }
        return await db.OneAsync("UPDATE scan_plan p SET status=@status,approved_by=coalesce(approved_by,@actor),approved_at=coalesce(approved_at,now()) WHERE id=@id RETURNING " + PlanColumns, ct, ("id", id), ("status", to), ("actor", actor.Id));
    }
    private static async Task<JsonNode> CreateRunAsync(DbSession db, Actor actor, long id, JsonObject input, CancellationToken ct)
    {
        JsonObject reference = await db.OneAsync("SELECT s.asset_id FROM scan_plan p JOIN asset_scope_revision s ON s.id=p.scope_id WHERE p.id=@id", ct, ("id", id));
        JsonObject asset = await RequireAssetAsync(db, actor, reference.Number("assetId"), ct);
        JsonObject plan = await db.OneAsync("SELECT " + PlanColumns + " FROM scan_plan p WHERE id=@id FOR UPDATE", ct, ("id", id));
        Access.Demand(Access.IsSecurity(actor, asset) || (actor.Has("SCAN_OPERATOR", asset.Number("id")) && plan.Number("operatorId") == actor.Id));
        Fields.Require(plan.Text("status") == "Active", "Chỉ tạo lần quét từ kế hoạch Active.");
        DateTime when = input.Date("scheduledFor");
        if (plan.Text("planKind") == "Verification") { Fields.Require(when == plan.Date("requestedFor"), "Lần xác minh phải đúng thời điểm yêu cầu."); }
        else { Fields.Require(Recurrence.Next(plan.Text("recurrenceRule"), plan.Text("timezone"), plan.Date("effectiveFrom"), when.AddTicks(-1), plan.NullableDate("effectiveUntil")) == when, "Thời điểm không khớp RRULE."); }
        await ValidatePlanAsync(db, plan, when, ct);
        return await db.OneAsync("INSERT INTO scan_run(plan_id,code,scheduled_for,scan_status) VALUES(@plan,@code,@when,'Queued') RETURNING id,code,plan_id,scan_status,scheduled_for,xmin::text::bigint AS version", ct,
            ("plan", id), ("code", "RUN-" + Guid.NewGuid().ToString("N")[..16]), ("when", when));
    }
    private static async Task<JsonNode> RunDetailAsync(DbSession db, Actor actor, long id, CancellationToken ct)
    {
        if (actor.Kind == "Service")
        {
            Fields.Require((await db.QueryAsync("SELECT r.id FROM scan_run r JOIN scan_plan p ON p.id=r.plan_id JOIN zap_scan_profile z ON z.id=p.zap_profile_id WHERE r.id=@id AND z.service_user_id=@actor", ct, ("id", id), ("actor", actor.Id))).Count > 0, "Tài khoản dịch vụ không thuộc lần quét này.", 403, "FORBIDDEN");
        }
        JsonObject reference = await db.OneAsync("SELECT s.asset_id FROM scan_run r JOIN scan_plan p ON p.id=r.plan_id JOIN asset_scope_revision s ON s.id=p.scope_id WHERE r.id=@id", ct, ("id", id));
        JsonObject runAsset = await RequireAssetAsync(db, actor, reference.Number("assetId"), ct);
        JsonObject run = await db.OneAsync("SELECT " + RunColumns + ",p.operator_id,p.plan_kind,p.remediation_ticket_id,s.asset_id FROM scan_run r JOIN scan_plan p ON p.id=r.plan_id JOIN asset_scope_revision s ON s.id=p.scope_id WHERE r.id=@id FOR UPDATE OF r", ct, ("id", id));
        bool assignedTicket = run.NullableNumber("remediationTicketId") is long ticket && actor.Has("REMEDIATOR", reference.Number("assetId")) && (await db.QueryAsync("SELECT id FROM remediation_ticket WHERE id=@id AND assignee_id=@actor", ct, ("id", ticket), ("actor", actor.Id))).Count > 0;
        Fields.Require(actor.Kind == "Service" || runAsset.Number("ownerId") == actor.Id || Access.IsSecurity(actor, runAsset) || actor.Has("AUDITOR", reference.Number("assetId")) || actor.Has("SCAN_OPERATOR", reference.Number("assetId")) && run.Number("operatorId") == actor.Id || assignedTicket, "Không tìm thấy trong nhiệm vụ được giao.", 404, "NOT_FOUND");
        run["findings"] = Page(await db.QueryAsync($"SELECT {FindingColumns},o.source_severity,o.source_confidence FROM finding_occurrence o JOIN finding f ON f.id=o.finding_id JOIN asset a ON a.id=f.asset_id WHERE o.scan_run_id=@id AND {AllowedFinding} ORDER BY f.id", ct, ("id", id), ("actor", actor.Id)));
        return run;
    }
    private static async Task<JsonNode> RunActionAsync(DbSession db, Actor actor, long id, string action, JsonObject input, string? etag, CancellationToken ct)
    {
        JsonObject run = (await RunDetailAsync(db, actor, id, ct)).AsObject();
        JsonObject asset = await RequireAssetAsync(db, actor, run.Number("assetId"), ct);
        Version(run, etag);
        if (action == "cancel")
        {
            Access.Demand(Access.IsSecurity(actor, asset) || actor.Id == run.Number("operatorId") && actor.Has("SCAN_OPERATOR", asset.Number("id")));
            _ = input.Text("reason"); Fields.Require(run.Text("scanStatus") is "Planned" or "Queued" or "Running", "Lần quét đã kết thúc.", 409, "STATE");
            await db.ExecuteAsync("UPDATE scan_run SET cancel_requested_at=now(),scan_status=CASE WHEN scan_status='Running' THEN scan_status ELSE 'Cancelled' END,finished_at=CASE WHEN scan_status='Running' THEN finished_at ELSE now() END,failure_message_masked=@reason WHERE id=@id", ct, ("id", id), ("reason", Protection.Mask(input.Text("reason"))));
        }
        else if (action == "record-result")
        {
            Access.Demand(actor.Id == run.Number("operatorId") && actor.Has("SCAN_OPERATOR", asset.Number("id")));
            Fields.Require(run.Text("scanStatus") is "Planned" or "Queued", "Chỉ ghi kết quả quét ngoài cho lần chưa chạy.", 409, "STATE");
            DateTime started = input.Date("startedAt"); DateTime finished = input.Date("finishedAt");
            Fields.Require(started <= finished && finished <= DateTime.UtcNow, "Thời gian chạy không hợp lệ.");
            JsonObject plan = await db.OneAsync("SELECT " + PlanColumns + " FROM scan_plan p WHERE id=@id", ct, ("id", run.Number("planId")));
            Fields.Require(plan.Text("status") == "Active" && started >= run.Date("scheduledFor") && started <= run.Date("scheduledFor").AddMinutes(5), "Kế hoạch không hiệu lực hoặc thời điểm bắt đầu lệch quá 5 phút so với lịch đã duyệt.");
            await ValidatePlanAsync(db, plan, started, ct);
            JsonObject limits = await db.OneAsync("SELECT max_duration_minutes FROM zap_scan_profile WHERE id=@id", ct, ("id", plan.Number("zapProfileId")));
            Fields.Require(finished <= started.AddMinutes(limits.Number("maxDurationMinutes")) && finished <= ScopeRules.WindowEnd(await WindowsAsync(db, plan.Number("scopeId"), ct), started), "Quét vượt thời lượng hoặc khung giờ đã duyệt.");
            string status = input.Text("scanStatus"); Fields.Require(status is "Completed" or "Failed", "Trạng thái ghi nhận không hợp lệ.");
            await db.ExecuteAsync("UPDATE scan_run SET scan_status=@status,started_at=@started,finished_at=@finished,failure_code=CASE WHEN @status='Failed' THEN 'EXTERNAL_SCAN_FAILED' END,failure_message_masked=@reason WHERE id=@id", ct,
                ("id", id), ("status", status), ("started", started), ("finished", finished), ("reason", Protection.Mask(input.Text("reason"))));
        }
        else if (action == "verification-decision") { return await DecideVerificationAsync(db, actor, run, input, ct); }
        else { throw new BusinessException(404, "NOT_FOUND", "Thao tác lần quét không tồn tại."); }
        return await db.OneAsync("SELECT " + RunColumns + " FROM scan_run r WHERE id=@id", ct, ("id", id));
    }

    public async Task<JsonNode> ImportAsync(DbSession db, Actor actor, long id, byte[] bytes, string format, string? etag, CancellationToken ct)
    {
        JsonObject run = (await RunDetailAsync(db, actor, id, ct)).AsObject();
        JsonObject asset = await RequireAssetAsync(db, actor, run.Number("assetId"), ct);
        Access.Demand(actor.Kind == "Service" || (actor.Id == run.Number("operatorId") && actor.Has("SCAN_OPERATOR", asset.Number("id"))));
        Version(run, etag);
        Fields.Require(run.Text("scanStatus") == "Completed", "Chỉ nhập báo cáo cho lần quét đã hoàn thành.");
        string hash = Convert.ToHexStringLower(SHA256.HashData(bytes));
        JsonObject stored = await db.OneAsync("SELECT report_sha256,report_object_key FROM scan_run WHERE id=@id", ct, ("id", id));
        if (stored.Optional("reportSha256") == hash) { return run; }
        Fields.Require(stored.Optional("reportObjectKey") is null, "Báo cáo gốc bất biến. Muốn nhập báo cáo khác phải tạo lần quét mới.", 409, "REPORT_IMMUTABLE");
        JsonObject plan = await db.OneAsync("SELECT scope_id FROM scan_plan WHERE id=@id", ct, ("id", run.Number("planId")));
        ZapReport report = ZapReports.Parse(bytes, format, await RulesAsync(db, plan.Number("scopeId"), ct));
        string objectKey = await protection.StoreAsync(bytes, ct);
        try
        {
            foreach (ZapRecord record in report.Records)
            {
                string identity = ScopeRules.Identity(asset.Number("id"), record.AlertId, record.Method, record.Url, record.Parameter);
                string fingerprint = protection.Fingerprint(identity);
                JsonObject finding = await db.OneAsync("INSERT INTO finding(asset_id,code,fingerprint,identity_key_ref,identity_ciphertext,source_alert_ref,affected_url_masked,parameter_masked,http_method,title_masked) VALUES(@asset,@code,@fingerprint,'local-aes-v1',@identity,@alert,@url,@parameter,@method,@title) ON CONFLICT(asset_id,fingerprint) DO UPDATE SET title_masked=finding.title_masked RETURNING id,status,decision_at", ct,
                    ("asset", asset.Number("id")), ("code", "F-" + Guid.NewGuid().ToString("N")[..16]), ("fingerprint", fingerprint), ("identity", protection.Encrypt(Encoding.UTF8.GetBytes(identity))),
                    ("alert", record.AlertId), ("url", Protection.Url(record.Url)), ("parameter", "[đã che]"), ("method", record.Method), ("title", Protection.Mask(record.Title)));
                int inserted = await db.ExecuteAsync("INSERT INTO finding_occurrence(finding_id,scan_run_id,record_locator,source_severity,source_confidence,source_attack_masked,source_evidence_masked,description_masked,remediation_guidance_masked,cwe_id,wasc_id) VALUES(@finding,@run,@locator,@severity,@confidence,'[đã che]','[đã che]',@description,@solution,@cwe,@wasc) ON CONFLICT(finding_id,scan_run_id) DO NOTHING", ct,
                    ("finding", finding.Number("id")), ("run", id), ("locator", record.Locator), ("severity", record.Severity), ("confidence", record.Confidence), ("description", Protection.Mask(record.Description)), ("solution", Protection.Mask(record.Solution)), ("cwe", record.Cwe), ("wasc", record.Wasc));
                if (inserted > 0 && finding.Text("status") == "Closed" && run.Date("startedAt") > finding.Date("decisionAt"))
                    await db.ExecuteAsync("UPDATE finding SET status='Reopened',decision_reason='Tái xuất hiện trong lần quét mới',decision_by=@actor,decision_at=now() WHERE id=@id", ct, ("actor", actor.Id), ("id", finding.Number("id")));
            }
            await db.ExecuteAsync("UPDATE scan_run SET report_status='Processed',report_object_key=@key,report_sha256=@hash,report_size_bytes=@size,report_format=@format,report_key_ref='local-aes-v1',report_received_at=now(),zap_version=@version WHERE id=@id", ct,
                ("id", id), ("key", objectKey), ("hash", hash), ("size", bytes.LongLength), ("format", report.Format), ("version", report.Version));
        }
        catch { protection.Remove(objectKey); throw; }
        return await RunDetailAsync(db, actor, id, ct);
    }

    public async Task<(byte[] Bytes, string Format)> RawReportAsync(DbSession db, Actor actor, long id, CancellationToken ct)
    {
        JsonObject run = (await RunDetailAsync(db, actor, id, ct)).AsObject();
        JsonObject asset = await RequireAssetAsync(db, actor, run.Number("assetId"), ct, true);
        _ = asset;
        JsonObject source = await db.OneAsync("SELECT report_object_key,report_format FROM scan_run WHERE id=@id AND report_object_key IS NOT NULL", ct, ("id", id));
        return (await protection.ReadAsync(source.Text("reportObjectKey"), ct), source.Text("reportFormat"));
    }

    public static async Task RejectReportAsync(DbSession db, Actor actor, long id, string code, CancellationToken ct)
    {
        JsonObject run = (await RunDetailAsync(db, actor, id, ct)).AsObject();
        JsonObject asset = await RequireAssetAsync(db, actor, run.Number("assetId"), ct);
        Access.Demand(actor.Kind == "Service" || actor.Id == run.Number("operatorId") && actor.Has("SCAN_OPERATOR", asset.Number("id")));
        await db.ExecuteAsync("UPDATE scan_run SET report_status='Rejected',failure_code=@code,failure_message_masked='Báo cáo không hợp lệ; không ghi nhận phát hiện nào.' WHERE id=@id AND scan_status='Completed' AND report_status<>'Processed' AND report_object_key IS NULL", ct, ("id", id), ("code", code));
    }
}
