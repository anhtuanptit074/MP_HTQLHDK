using System.Text.Json.Nodes;

namespace Htqlhdk.Core;

public sealed partial class CoreService
{
    private static async Task<JsonNode> FindingDetailAsync(DbSession db, Actor actor, long id, CancellationToken ct)
    {
        JsonObject finding = await RequireFindingAsync(db, actor, id, ct);
        finding["occurrences"] = Page(await db.QueryAsync("SELECT o.id,o.scan_run_id,o.source_severity,o.source_confidence,o.description_masked,o.remediation_guidance_masked,o.cwe_id,o.wasc_id,r.started_at FROM finding_occurrence o JOIN scan_run r ON r.id=o.scan_run_id WHERE finding_id=@id ORDER BY o.id", ct, ("id", id)));
        finding["tickets"] = Page(await db.QueryAsync("SELECT " + TicketColumns + " FROM remediation_ticket t WHERE finding_id=@id", ct, ("id", id)));
        return finding;
    }
    private static async Task<JsonNode> AssessAsync(DbSession db, Actor actor, long id, JsonObject input, string? etag, CancellationToken ct)
    {
        JsonObject finding = await RequireFindingAsync(db, actor, id, ct, true); Version(finding, etag);
        Fields.Require(finding.Text("status") is "New" or "Triaged" or "Assigned" or "In Progress" or "Reopened", "Phát hiện không cho phép đánh giá mức độ lúc này.", 409, "STATE");
        bool importer = (await db.QueryAsync("SELECT o.id FROM finding_occurrence o JOIN scan_run r ON r.id=o.scan_run_id JOIN scan_plan p ON p.id=r.plan_id WHERE o.finding_id=@id AND p.operator_id=@actor", ct, ("id", id), ("actor", actor.Id))).Count > 0;
        Fields.Require(!importer, "Người nhập kết quả không tự xác nhận Final Severity.");
        string severity = input.Text("finalSeverity"); Fields.Require(severity is "Informational" or "Low" or "Medium" or "High" or "Critical", "Mức độ không hợp lệ.");
        return await db.OneAsync("UPDATE finding f SET final_severity=@severity,severity_reason=@reason,assessed_by=@actor,assessed_at=now(),status=CASE WHEN status='New' THEN 'Triaged' ELSE status END WHERE id=@id RETURNING " + FindingColumns, ct,
            ("id", id), ("severity", severity), ("reason", Protection.Mask(input.Text("reason"))), ("actor", actor.Id));
    }
    private static async Task<JsonNode> DispositionAsync(DbSession db, Actor actor, long id, JsonObject input, string? etag, CancellationToken ct)
    {
        JsonObject finding = await RequireFindingAsync(db, actor, id, ct, true); Version(finding, etag);
        Fields.Require(finding.Text("status") is "New" or "Triaged", "Chỉ kết luận kết quả sai/ngoài phạm vi trước phân công.", 409, "STATE");
        string to = input.Text("toStatus"); Fields.Require(to is "False Positive" or "Cancelled", "Kết luận không hợp lệ.");
        return await db.OneAsync("UPDATE finding f SET status=@status,decision_reason=@reason,decision_by=@actor,decision_at=now() WHERE id=@id RETURNING " + FindingColumns, ct,
            ("id", id), ("status", to), ("reason", Protection.Mask(input.Text("reason"))), ("actor", actor.Id));
    }
    private static async Task<JsonNode> SaveTicketAsync(DbSession db, Actor actor, long? findingId, long? ticketId, JsonObject input, string? etag, CancellationToken ct)
    {
        if (findingId is null) { findingId = (await db.OneAsync("SELECT finding_id FROM remediation_ticket WHERE id=@id", ct, ("id", ticketId))).Number("findingId"); }
        JsonObject finding = await RequireFindingAsync(db, actor, findingId.Value, ct, true);
        await RequireEligibleAsync(db, input.Number("assigneeId"), "REMEDIATOR", finding.Number("assetId"), ct);
        Fields.Require(input.Date("dueAt") > DateTime.UtcNow, "Hạn xử lý phải ở tương lai.");
        _ = input.Text("reason");
        if (ticketId is null)
        {
            Version(finding, etag); Fields.Require(finding.Text("status") is "Triaged" or "Reopened", "Phát hiện cần được phân loại trước khi phân công.", 409, "STATE");
            List<JsonObject> existing = await db.QueryAsync("SELECT id FROM remediation_ticket WHERE finding_id=@id", ct, ("id", findingId));
            if (existing.Count > 0) { ticketId = existing[0].Number("id"); }
            else
            {
                JsonObject ticket = await db.OneAsync("INSERT INTO remediation_ticket(finding_id,code,assignee_id,assigned_by,due_at,created_by,updated_by) VALUES(@finding,@code,@assignee,@actor,@due,@actor,@actor) RETURNING id", ct,
                    ("finding", findingId), ("code", "T-" + Guid.NewGuid().ToString("N")[..16]), ("assignee", input.Number("assigneeId")), ("actor", actor.Id), ("due", input.Date("dueAt")));
                ticketId = ticket.Number("id");
            }
        }
        else
        {
            JsonObject old = await db.OneAsync("SELECT " + TicketColumns + " FROM remediation_ticket t WHERE id=@id FOR UPDATE", ct, ("id", ticketId)); Version(old, etag);
            Fields.Require(finding.Text("status") is "Assigned" or "In Progress" or "Reopened", "Không đổi phân công khi đang xác minh hoặc đã kết thúc.", 409, "STATE");
        }
        await db.ExecuteAsync("UPDATE remediation_ticket SET assignee_id=@assignee,assigned_by=@actor,assigned_at=now(),due_at=@due,updated_by=@actor,updated_at=now() WHERE id=@id", ct,
            ("id", ticketId), ("assignee", input.Number("assigneeId")), ("actor", actor.Id), ("due", input.Date("dueAt")));
        if (finding.Text("status") is "Triaged" or "Reopened")
            await db.ExecuteAsync("UPDATE finding SET status='Assigned' WHERE id=@id", ct, ("id", findingId));
        return await TicketDetailAsync(db, actor, ticketId.Value, ct);
    }
    private static async Task<JsonNode> TicketDetailAsync(DbSession db, Actor actor, long id, CancellationToken ct)
    {
        JsonObject reference = await db.OneAsync("SELECT finding_id FROM remediation_ticket WHERE id=@id", ct, ("id", id));
        JsonObject finding = await RequireFindingAsync(db, actor, reference.Number("findingId"), ct);
        JsonObject ticket = await db.OneAsync("SELECT " + TicketColumns + " FROM remediation_ticket t WHERE id=@id FOR UPDATE", ct, ("id", id));
        JsonObject evidence = await db.OneAsync("SELECT evidence_object_key IS NOT NULL AS has_evidence,evidence_sha256 FROM remediation_ticket WHERE id=@id", ct, ("id", id));
        ticket["hasEvidence"] = evidence["hasEvidence"]?.DeepClone(); ticket["evidenceSha256"] = evidence["evidenceSha256"]?.DeepClone();
        ticket["finding"] = finding;
        ticket["verificationRuns"] = Page(await db.QueryAsync("SELECT " + RunColumns + " FROM scan_run r JOIN scan_plan p ON p.id=r.plan_id WHERE p.remediation_ticket_id=@id ORDER BY r.id", ct, ("id", id)));
        return ticket;
    }
    private static async Task<JsonNode> TicketProgressAsync(DbSession db, Actor actor, long id, JsonObject input, string? etag, bool submit, CancellationToken ct)
    {
        JsonObject ticket = (await TicketDetailAsync(db, actor, id, ct)).AsObject(); Version(ticket, etag);
        JsonObject finding = ticket["finding"]!.AsObject();
        Access.Demand(ticket.Number("assigneeId") == actor.Id && actor.Has("REMEDIATOR", finding.Number("assetId")));
        Fields.Require(finding.Text("status") is "Assigned" or "In Progress", "Phiếu không ở trạng thái khắc phục.", 409, "STATE");
        string action = Protection.Mask(input.Text("remediationActionMasked", ticket.Optional("remediationActionMasked") ?? "Chưa cập nhật"));
        string evidence = Protection.Mask(input.Text("evidenceSummaryMasked", ticket.Optional("evidenceSummaryMasked") ?? "Chưa có bằng chứng"));
        if (submit) { Fields.Require(input.Optional("remediationActionMasked") is { Length: > 2 } && input.Optional("evidenceSummaryMasked") is { Length: > 2 }, "Cần biện pháp và bằng chứng thực tế trước khi yêu cầu xác minh."); }
        long progress = submit ? 100 : input.Number("progressPercent"); Fields.Require(progress is >= 0 and <= 100, "Tiến độ phải từ 0 đến 100.");
        await db.ExecuteAsync("UPDATE remediation_ticket SET progress_percent=@progress,root_cause_masked=@root,remediation_action_masked=@action,evidence_summary_masked=@evidence,remediated_at=CASE WHEN @submit THEN now() ELSE remediated_at END,updated_by=@actor,updated_at=now() WHERE id=@id", ct,
            ("id", id), ("progress", progress), ("root", Protection.Mask(input.Optional("rootCauseMasked") ?? "")), ("action", action), ("evidence", evidence), ("submit", submit), ("actor", actor.Id));
        if (finding.Text("status") == "Assigned") { await db.ExecuteAsync("UPDATE finding SET status='In Progress' WHERE id=@id", ct, ("id", finding.Number("id"))); }
        if (submit) { await db.ExecuteAsync("UPDATE finding SET status='Pending Verification' WHERE id=@id", ct, ("id", finding.Number("id"))); }
        return await TicketDetailAsync(db, actor, id, ct);
    }
    private static async Task<JsonNode> VerificationPlanAsync(DbSession db, Actor actor, long ticketId, JsonObject input, CancellationToken ct)
    {
        JsonObject ticket = (await TicketDetailAsync(db, actor, ticketId, ct)).AsObject();
        JsonObject finding = ticket["finding"]!.AsObject(); await RequireAssetAsync(db, actor, finding.Number("assetId"), ct, true);
        Fields.Require(finding.Text("status") == "Pending Verification" && ticket.NullableDate("remediatedAt") is not null, "Phiếu chưa được gửi xác minh.");
        await RequireEligibleAsync(db, input.Number("operatorId"), "SCAN_OPERATOR", finding.Number("assetId"), ct);
        JsonObject scope = await db.OneAsync("SELECT asset_id,status FROM asset_scope_revision WHERE id=@id", ct, ("id", input.Number("scopeId")));
        Fields.Require(scope.Number("assetId") == finding.Number("assetId") && scope.Text("status") == "Approved", "Phạm vi xác minh phải cùng tài sản và đã duyệt.");
        DateTime requested = input.Date("requestedFor"); Fields.Require(requested >= ticket.Date("remediatedAt"), "Quét xác minh phải sau khắc phục.");
        JsonObject plan = await db.OneAsync("INSERT INTO scan_plan(code,name,plan_kind,scope_id,zap_profile_id,operator_id,remediation_ticket_id,requested_for,created_by) VALUES(@code,@name,'Verification',@scope,@profile,@operator,@ticket,@when,@actor) RETURNING id,scope_id,zap_profile_id,operator_id,requested_for", ct,
            ("code", "VERIFY-" + Guid.NewGuid().ToString("N")[..16]), ("name", "Xác minh " + ticket.Text("code")), ("scope", input.Number("scopeId")), ("profile", input.Number("zapProfileId")), ("operator", input.Number("operatorId")), ("ticket", ticketId), ("when", requested), ("actor", actor.Id));
        await ValidatePlanAsync(db, plan, requested, ct);
        return await db.OneAsync("UPDATE scan_plan p SET status='Active',approved_by=@actor,approved_at=now() WHERE id=@id RETURNING " + PlanColumns, ct, ("id", plan.Number("id")), ("actor", actor.Id));
    }
    private static async Task<JsonNode> DecideVerificationAsync(DbSession db, Actor actor, JsonObject run, JsonObject input, CancellationToken ct)
    {
        await RequireAssetAsync(db, actor, run.Number("assetId"), ct, true);
        Fields.Require(run.Text("planKind") == "Verification" && run.Text("scanStatus") == "Completed" && run.Text("reportStatus") == "Processed", "Cần lần quét xác minh và báo cáo đã xử lý thành công.");
        Fields.Require(run.Optional("verificationOutcome") is null, "Kết luận xác minh đã bất biến.", 409, "STATE");
        JsonObject ticket = await db.OneAsync("SELECT finding_id,assignee_id,updated_by,remediated_at FROM remediation_ticket WHERE id=@id FOR UPDATE", ct, ("id", run.Number("remediationTicketId")));
        JsonObject finding = await RequireFindingAsync(db, actor, ticket.Number("findingId"), ct, true);
        Fields.Require(finding.Text("status") == "Pending Verification", "Phát hiện không chờ xác minh.");
        Fields.Require(actor.Id != ticket.Number("assigneeId") && actor.Id != ticket.Number("updatedBy"), "Người khắc phục không được tự đóng phát hiện.");
        Fields.Require(run.Date("startedAt") >= ticket.Date("remediatedAt"), "Lần quét phải sau bản khắc phục hiện hành.");
        bool present = (await db.QueryAsync("SELECT id FROM finding_occurrence WHERE finding_id=@finding AND scan_run_id=@run", ct, ("finding", finding.Number("id")), ("run", run.Number("id")))).Count > 0;
        bool coverage = input.Flag("coverageConfirmed"); Fields.Require(coverage || present, "Chưa đủ coverage để kết luận không còn lỗ hổng.");
        string outcome = present ? "Reopened" : "Closed";
        string reason = Protection.Mask(input.Text("reason"));
        await db.ExecuteAsync("UPDATE scan_run SET coverage_confirmed=@coverage,fingerprint_present=@present,verification_outcome=@outcome,verification_reason=@reason,evaluated_by=@actor,evaluated_at=now() WHERE id=@id; UPDATE finding SET status=@outcome,decision_reason=@reason,decision_by=@actor,decision_at=now() WHERE id=@finding", ct,
            ("id", run.Number("id")), ("finding", finding.Number("id")), ("coverage", coverage), ("present", present), ("outcome", outcome), ("reason", reason), ("actor", actor.Id));
        return await FindingDetailAsync(db, actor, finding.Number("id"), ct);
    }
    private static async Task<JsonNode> AuditsAsync(DbSession db, Actor actor, long after, int limit, CancellationToken ct)
    {
        bool administrator = actor.Has("ADMIN");
        bool permitted = administrator || actor.Eligible("SECURITY") || actor.Eligible("AUDITOR"); Access.Demand(permitted);
        // Resolve each object's current asset through relational data. NULL grants are never wildcards.
        string resolved = "CASE l.object_type WHEN 'assets' THEN nullif(l.object_id,'')::bigint WHEN 'scopes' THEN (SELECT asset_id FROM asset_scope_revision WHERE id=nullif(l.object_id,'')::bigint) WHEN 'findings' THEN (SELECT asset_id FROM finding WHERE id=nullif(l.object_id,'')::bigint) WHEN 'tickets' THEN (SELECT f.asset_id FROM remediation_ticket t JOIN finding f ON f.id=t.finding_id WHERE t.id=nullif(l.object_id,'')::bigint) WHEN 'runs' THEN (SELECT s.asset_id FROM scan_run r JOIN scan_plan p ON p.id=r.plan_id JOIN asset_scope_revision s ON s.id=p.scope_id WHERE r.id=nullif(l.object_id,'')::bigint) WHEN 'plans' THEN (SELECT s.asset_id FROM scan_plan p JOIN asset_scope_revision s ON s.id=p.scope_id WHERE p.id=nullif(l.object_id,'')::bigint) ELSE NULL END";
        return Page(await db.QueryAsync("SELECT l.id,l.actor_id,l.occurred_at,l.category,l.action,l.object_type,l.object_id,l.outcome,l.before_masked,l.after_masked,l.reason_masked,l.correlation_id FROM audit_log l WHERE l.id>@after AND " +
            (administrator ? "l.category IN('Permission','Security')" : $"(l.category='Business' AND EXISTS(SELECT 1 FROM user_role ur WHERE ur.user_id=@actor AND ur.asset_id=({resolved}) AND ur.role_code IN('SECURITY','AUDITOR') AND ur.revoked_at IS NULL))") + " ORDER BY l.id LIMIT @limit", ct, ("after", after), ("limit", limit), ("actor", actor.Id)));
    }
    private static async Task<JsonNode> WorklistAsync(DbSession db, Actor actor, CancellationToken ct)
    {
        return new JsonObject
        {
            ["urgentFindings"] = Page(await db.QueryAsync($"SELECT {FindingColumns},a.name AS asset_name FROM finding f JOIN asset a ON a.id=f.asset_id WHERE {AllowedFinding} AND f.final_severity IN('High','Critical') AND f.status NOT IN('Closed','False Positive','Cancelled') ORDER BY f.id LIMIT 100", ct, ("actor", actor.Id))),
            ["overdueTickets"] = Page(await db.QueryAsync($"SELECT {TicketColumns},f.title_masked,f.status FROM remediation_ticket t JOIN finding f ON f.id=t.finding_id JOIN asset a ON a.id=f.asset_id WHERE {AllowedFinding} AND t.due_at<now() AND f.status NOT IN('Closed','False Positive','Cancelled') ORDER BY t.due_at LIMIT 100", ct, ("actor", actor.Id))),
            ["failedRuns"] = Page(await db.QueryAsync($"SELECT {RunColumns},s.asset_id FROM scan_run r JOIN scan_plan p ON p.id=r.plan_id JOIN asset_scope_revision s ON s.id=p.scope_id JOIN asset a ON a.id=s.asset_id WHERE {AllowedPlan} AND r.scan_status IN('Failed','Timed Out') ORDER BY r.id DESC LIMIT 100", ct, ("actor", actor.Id)))
        };
    }
    private static async Task<JsonNode> SummaryAsync(DbSession db, Actor actor, CancellationToken ct) => new JsonObject
    {
        ["byStatus"] = Page(await db.QueryAsync($"SELECT f.status,count(*) AS count FROM finding f JOIN asset a ON a.id=f.asset_id WHERE {AllowedFinding} GROUP BY f.status ORDER BY f.status", ct, ("actor", actor.Id))),
        ["bySeverity"] = Page(await db.QueryAsync($"SELECT coalesce(f.final_severity,'Unassessed') AS severity,count(*) AS count FROM finding f JOIN asset a ON a.id=f.asset_id WHERE {AllowedFinding} GROUP BY f.final_severity ORDER BY f.final_severity", ct, ("actor", actor.Id)))
    };
}
