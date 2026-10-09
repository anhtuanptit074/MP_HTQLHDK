using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

namespace Htqlhdk.Core;

public sealed partial class CoreService(Protection protection)
{
    private static readonly System.Text.Json.JsonSerializerOptions WebJson = new(System.Text.Json.JsonSerializerDefaults.Web);
    private static readonly string[] ExportKeys = ["code", "assetCode", "titleMasked", "affectedUrlMasked", "finalSeverity", "status"];
    public static long ParseId(string value) => Id(value);
    internal const string AssetColumns = "a.id,a.code,a.name,a.base_url,a.environment,a.owner_id,a.security_officer_id,a.criticality,a.data_classification,a.internet_exposed,a.status,a.created_at,a.updated_at,a.xmin::text::bigint AS version";
    internal const string FindingColumns = "f.id,f.code,f.asset_id,f.title_masked,f.affected_url_masked,f.parameter_masked,f.http_method,f.final_severity,f.severity_reason,f.assessed_by,f.assessed_at,f.status,f.decision_reason,f.decision_at,f.row_version AS version";
    internal const string TicketColumns = "t.id,t.code,t.finding_id,t.assignee_id,t.assigned_by,t.due_at,t.progress_percent,t.root_cause_masked,t.remediation_action_masked,t.evidence_summary_masked,t.remediated_at,t.updated_by,t.updated_at,t.xmin::text::bigint AS version";
    internal const string PlanColumns = "p.id,p.code,p.version_no,p.name,p.plan_kind,p.scope_id,p.zap_profile_id,p.operator_id,p.status,p.recurrence_rule,p.timezone,p.effective_from,p.effective_until,p.remediation_ticket_id,p.requested_for,p.approved_by,p.created_at,p.xmin::text::bigint AS version";
    internal const string RunColumns = "r.id,r.code,r.plan_id,r.scheduled_for,r.scan_status,r.requested_at,r.started_at,r.heartbeat_at,r.finished_at,r.zap_version,r.exit_code,r.failure_code,r.failure_message_masked,r.report_status,r.report_format,r.report_received_at,r.coverage_confirmed,r.fingerprint_present,r.verification_outcome,r.verification_reason,r.evaluated_by,r.evaluated_at,r.xmin::text::bigint AS version";
    internal const string ProfileColumns = "z.id,z.code,z.name,z.version_no,z.status,z.spider_kind,z.passive_scan_enabled,z.active_scan_enabled,z.max_duration_minutes,z.request_delay_ms,z.thread_per_host,z.auth_kind,z.auth_header_name,z.report_format,z.service_user_id,z.created_by,z.approved_by,z.approved_at,z.xmin::text::bigint AS version";
    internal const string ScopeColumns = "s.id,s.asset_id,s.version_no,s.status,s.change_reason,s.created_by,s.confirmed_by,s.confirmed_at,s.decided_by,s.decided_at,s.decision_reason,s.effective_from,s.effective_until,s.xmin::text::bigint AS version";
    internal const string AllowedAsset = "(a.owner_id=@actor OR (a.security_officer_id=@actor AND EXISTS(SELECT 1 FROM user_role ur WHERE ur.user_id=@actor AND ur.asset_id=a.id AND ur.role_code='SECURITY' AND ur.revoked_at IS NULL)) OR EXISTS(SELECT 1 FROM user_role ur WHERE ur.user_id=@actor AND ur.asset_id=a.id AND ur.role_code='AUDITOR' AND ur.revoked_at IS NULL) OR (EXISTS(SELECT 1 FROM user_role ur WHERE ur.user_id=@actor AND ur.asset_id=a.id AND ur.role_code='SCAN_OPERATOR' AND ur.revoked_at IS NULL) AND EXISTS(SELECT 1 FROM scan_plan pp JOIN asset_scope_revision ss ON ss.id=pp.scope_id WHERE ss.asset_id=a.id AND pp.operator_id=@actor)) OR (EXISTS(SELECT 1 FROM user_role ur WHERE ur.user_id=@actor AND ur.asset_id=a.id AND ur.role_code='REMEDIATOR' AND ur.revoked_at IS NULL) AND EXISTS(SELECT 1 FROM remediation_ticket tt JOIN finding ff ON ff.id=tt.finding_id WHERE ff.asset_id=a.id AND tt.assignee_id=@actor)))";
    internal const string GeneralRead = "(a.owner_id=@actor OR (a.security_officer_id=@actor AND EXISTS(SELECT 1 FROM user_role ur WHERE ur.user_id=@actor AND ur.asset_id=a.id AND ur.role_code='SECURITY' AND ur.revoked_at IS NULL)) OR EXISTS(SELECT 1 FROM user_role ur WHERE ur.user_id=@actor AND ur.asset_id=a.id AND ur.role_code='AUDITOR' AND ur.revoked_at IS NULL))";
    internal const string AllowedFinding = "(" + GeneralRead + " OR (EXISTS(SELECT 1 FROM user_role ur WHERE ur.user_id=@actor AND ur.asset_id=a.id AND ur.role_code='SCAN_OPERATOR' AND ur.revoked_at IS NULL) AND EXISTS(SELECT 1 FROM finding_occurrence oo JOIN scan_run rr ON rr.id=oo.scan_run_id JOIN scan_plan pp ON pp.id=rr.plan_id WHERE oo.finding_id=f.id AND pp.operator_id=@actor)) OR (EXISTS(SELECT 1 FROM user_role ur WHERE ur.user_id=@actor AND ur.asset_id=a.id AND ur.role_code='REMEDIATOR' AND ur.revoked_at IS NULL) AND EXISTS(SELECT 1 FROM remediation_ticket tt WHERE tt.finding_id=f.id AND tt.assignee_id=@actor)))";
    internal const string AllowedPlan = "(" + GeneralRead + " OR (p.operator_id=@actor AND EXISTS(SELECT 1 FROM user_role ur WHERE ur.user_id=@actor AND ur.asset_id=a.id AND ur.role_code='SCAN_OPERATOR' AND ur.revoked_at IS NULL)) OR (EXISTS(SELECT 1 FROM user_role ur WHERE ur.user_id=@actor AND ur.asset_id=a.id AND ur.role_code='REMEDIATOR' AND ur.revoked_at IS NULL) AND EXISTS(SELECT 1 FROM remediation_ticket tt WHERE tt.id=p.remediation_ticket_id AND tt.assignee_id=@actor)))";

    public static async Task<JsonNode> DispatchAsync(DbSession db, Actor actor, string method, string[] path, JsonObject input, string? etag,
        long after, int pageSize, CancellationToken ct)
    {
        return (method, path) switch
        {
            ("GET", ["context"]) => await ContextAsync(db, actor, ct),
            ("GET", ["users"]) => Page(await db.QueryAsync(actor.Has("ADMIN") ? "SELECT id,username,display_name,account_kind,is_active,xmin::text::bigint AS version FROM app_user ORDER BY id" : "SELECT id,display_name,account_kind FROM app_user WHERE is_active ORDER BY id", ct)),
            ("POST", ["users"]) => await CreateUserAsync(db, actor, input, ct),
            ("PATCH", ["users", var id]) => await UpdateUserAsync(db, actor, Id(id), input, etag, ct),
            ("GET", ["access-grants"]) => await GrantsAsync(db, actor, ct),
            ("GET", ["access-assets"]) => await AccessAssetsAsync(db, actor, ct),
            ("POST", ["access-grants"]) => await GrantAsync(db, actor, input, ct),
            ("POST", ["access-grants", var id, "revoke"]) => await RevokeAsync(db, actor, Id(id), ct),
            ("GET", ["assets"]) => Page(await db.QueryAsync($"SELECT {AssetColumns} FROM asset a WHERE {AllowedAsset} AND a.id>@after ORDER BY a.id LIMIT @limit", ct, ("actor", actor.Id), ("after", after), ("limit", pageSize))),
            ("POST", ["assets"]) => await CreateAssetAsync(db, actor, input, ct),
            ("GET", ["assets", var id]) => await AssetDetailAsync(db, actor, Id(id), ct),
            ("PATCH", ["assets", var id]) => await UpdateAssetAsync(db, actor, Id(id), input, etag, ct),
            ("POST", ["assets", var id, "assignment"]) => await AssignAssetAsync(db, actor, Id(id), input, etag, ct),
            ("POST", ["assets", var id, "transitions"]) => await TransitionAssetAsync(db, actor, Id(id), input, etag, ct),
            ("POST", ["assets", var id, "scopes"]) => await SaveScopeAsync(db, actor, Id(id), null, input, null, ct),
            ("PUT", ["assets", var id, "scopes", var scope]) => await SaveScopeAsync(db, actor, Id(id), Id(scope), input, etag, ct),
            ("POST", ["assets", var id, "scopes", var scope, var action]) => await ScopeActionAsync(db, actor, Id(id), Id(scope), action, input, etag, ct),
            ("GET", ["profiles"]) => await ProfilesAsync(db, actor, ct),
            ("POST", ["profiles"]) => await SaveProfileAsync(db, actor, null, input, null, ct),
            ("PUT", ["profiles", var id]) => await SaveProfileAsync(db, actor, Id(id), input, etag, ct),
            ("POST", ["profiles", var id, var action]) => await ProfileActionAsync(db, actor, Id(id), action, input, etag, ct),
            ("GET", ["plans"]) => await PlansAsync(db, actor, after, pageSize, ct),
            ("POST", ["plans"]) => await SavePlanAsync(db, actor, null, input, null, ct),
            ("PUT", ["plans", var id]) => await SavePlanAsync(db, actor, Id(id), input, etag, ct),
            ("POST", ["plans", var id, "transitions"]) => await PlanActionAsync(db, actor, Id(id), input, etag, ct),
            ("POST", ["plans", var id, "runs"]) => await CreateRunAsync(db, actor, Id(id), input, ct),
            ("GET", ["runs"]) => Page(await db.QueryAsync($"SELECT {RunColumns},s.asset_id,p.name AS plan_name,p.plan_kind,p.operator_id FROM scan_run r JOIN scan_plan p ON p.id=r.plan_id JOIN asset_scope_revision s ON s.id=p.scope_id JOIN asset a ON a.id=s.asset_id WHERE {AllowedPlan} AND r.id>@after ORDER BY r.id LIMIT @limit", ct, ("actor", actor.Id), ("after", after), ("limit", pageSize))),
            ("GET", ["runs", var id]) => await RunDetailAsync(db, actor, Id(id), ct),
            ("POST", ["runs", var id, var action]) => await RunActionAsync(db, actor, Id(id), action, input, etag, ct),
            ("GET", ["findings"]) => Page(await db.QueryAsync($"SELECT {FindingColumns},a.name AS asset_name,(SELECT o.source_severity FROM finding_occurrence o WHERE o.finding_id=f.id ORDER BY CASE o.source_severity WHEN 'High' THEN 3 WHEN 'Medium' THEN 2 WHEN 'Low' THEN 1 ELSE 0 END DESC LIMIT 1) AS source_severity,(SELECT max(o.source_confidence) FROM finding_occurrence o WHERE o.finding_id=f.id) AS source_confidence FROM finding f JOIN asset a ON a.id=f.asset_id WHERE {AllowedFinding} AND f.id>@after ORDER BY f.id LIMIT @limit", ct, ("actor", actor.Id), ("after", after), ("limit", pageSize))),
            ("GET", ["findings", var id]) => await FindingDetailAsync(db, actor, Id(id), ct),
            ("POST", ["findings", var id, "assess"]) => await AssessAsync(db, actor, Id(id), input, etag, ct),
            ("POST", ["findings", var id, "disposition"]) => await DispositionAsync(db, actor, Id(id), input, etag, ct),
            ("POST", ["findings", var id, "tickets"]) => await SaveTicketAsync(db, actor, Id(id), null, input, etag, ct),
            ("GET", ["tickets"]) => Page(await db.QueryAsync($"SELECT {TicketColumns},f.asset_id,f.status,f.title_masked,f.final_severity,a.name AS asset_name FROM remediation_ticket t JOIN finding f ON f.id=t.finding_id JOIN asset a ON a.id=f.asset_id WHERE {AllowedFinding} AND t.id>@after ORDER BY t.id LIMIT @limit", ct, ("actor", actor.Id), ("after", after), ("limit", pageSize))),
            ("GET", ["tickets", var id]) => await TicketDetailAsync(db, actor, Id(id), ct),
            ("POST", ["tickets", var id, "assignment"]) => await SaveTicketAsync(db, actor, null, Id(id), input, etag, ct),
            ("POST", ["tickets", var id, "progress"]) => await TicketProgressAsync(db, actor, Id(id), input, etag, false, ct),
            ("POST", ["tickets", var id, "submit-verification"]) => await TicketProgressAsync(db, actor, Id(id), input, etag, true, ct),
            ("POST", ["tickets", var id, "verification-plans"]) => await VerificationPlanAsync(db, actor, Id(id), input, ct),
            ("GET", ["audit-logs"]) => await AuditsAsync(db, actor, after, pageSize, ct),
            ("GET", ["notifications"]) => await WorklistAsync(db, actor, ct),
            ("GET", ["reports"]) => await SummaryAsync(db, actor, ct),
            _ => throw new BusinessException(404, "NOT_FOUND", "API không tồn tại.")
        };
    }

    internal static long Id(string value) => long.TryParse(value, CultureInfo.InvariantCulture, out long id) && id > 0 ? id : throw new BusinessException(404, "NOT_FOUND", "ID không hợp lệ.");
    internal static JsonObject Page(List<JsonObject> rows) => new() { ["items"] = new JsonArray(rows.Select(r => (JsonNode)r).ToArray()), ["page"] = new JsonObject { ["after"] = rows.LastOrDefault()?.NullableNumber("id") } };
    internal static void Version(JsonObject row, string? etag)
    {
        Fields.Require(etag is not null, "Cần phiên bản hiện hành (If-Match).", 428, "VERSION_REQUIRED");
        Fields.Require(etag == "\"" + row.Number("version").ToString(CultureInfo.InvariantCulture) + "\"", "Dữ liệu đã thay đổi. Hãy tải lại trước khi thao tác.", 412, "STALE_VERSION");
    }
    internal static async Task<JsonObject> RequireAssetAsync(DbSession db, Actor actor, long id, CancellationToken ct, bool write = false)
    {
        JsonObject asset = await db.OneAsync($"SELECT {AssetColumns} FROM asset a WHERE a.id=@id FOR UPDATE", ct, ("id", id));
        bool assigned = (await db.QueryAsync("SELECT id FROM scan_plan WHERE operator_id=@actor AND scope_id IN(SELECT id FROM asset_scope_revision WHERE asset_id=@asset) UNION SELECT t.id FROM remediation_ticket t JOIN finding f ON f.id=t.finding_id WHERE t.assignee_id=@actor AND f.asset_id=@asset UNION SELECT p.id FROM scan_plan p JOIN zap_scan_profile z ON z.id=p.zap_profile_id JOIN asset_scope_revision s ON s.id=p.scope_id WHERE z.service_user_id=@actor AND s.asset_id=@asset", ct, ("actor", actor.Id), ("asset", id))).Count > 0;
        Fields.Require(Access.CanReadAsset(actor, asset, assigned), "Không tìm thấy trong phạm vi được phép.", 404, "NOT_FOUND");
        if (write) { Access.Demand(Access.IsSecurity(actor, asset)); }
        return asset;
    }
    internal static async Task<JsonObject> RequireFindingAsync(DbSession db, Actor actor, long id, CancellationToken ct, bool write = false)
    {
        JsonObject reference = await db.OneAsync("SELECT asset_id FROM finding WHERE id=@id", ct, ("id", id));
        JsonObject asset = await RequireAssetAsync(db, actor, reference.Number("assetId"), ct, write);
        JsonObject finding = await db.OneAsync($"SELECT {FindingColumns} FROM finding f WHERE f.id=@id FOR UPDATE", ct, ("id", id));
        bool operatorSource = actor.Has("SCAN_OPERATOR", asset.Number("id")) && (await db.QueryAsync("SELECT o.id FROM finding_occurrence o JOIN scan_run r ON r.id=o.scan_run_id JOIN scan_plan p ON p.id=r.plan_id WHERE o.finding_id=@id AND p.operator_id=@actor", ct, ("id", id), ("actor", actor.Id))).Count > 0;
        bool general = asset.Number("ownerId") == actor.Id || Access.IsSecurity(actor, asset) || actor.Has("AUDITOR", asset.Number("id")) || operatorSource;
        bool assigned = (await db.QueryAsync("SELECT id FROM remediation_ticket WHERE finding_id=@id AND assignee_id=@actor", ct, ("id", id), ("actor", actor.Id))).Count > 0;
        Fields.Require(general || (actor.Has("REMEDIATOR", asset.Number("id")) && assigned), "Không tìm thấy trong phạm vi được phép.", 404, "NOT_FOUND");
        return finding;
    }
    private static async Task RequireEligibleAsync(DbSession db, long user, string role, long? asset, CancellationToken ct)
    {
        JsonObject target = await db.OneAsync("SELECT id,account_kind FROM app_user WHERE id=@id AND is_active FOR SHARE", ct, ("id", user));
        Fields.Require(target.Text("accountKind") == "Human", "Người nhận nhiệm vụ phải là tài khoản người dùng.");
        Fields.Require((await db.QueryAsync("SELECT id FROM user_role WHERE user_id=@id AND role_code=@role AND revoked_at IS NULL AND (@asset::bigint IS NULL OR asset_id=@asset) FOR SHARE", ct, ("id", user), ("role", role), ("asset", asset))).Count > 0, "Người được chọn chưa có vai trò phù hợp trong phạm vi tài sản.");
    }
    private static async Task<JsonNode> ContextAsync(DbSession db, Actor actor, CancellationToken ct) => new JsonObject
    {
        ["userId"] = actor.Id, ["displayName"] = actor.Name, ["grants"] = System.Text.Json.JsonSerializer.SerializeToNode(actor.Grants, WebJson),
        ["assets"] = Page(await db.QueryAsync($"SELECT {AssetColumns} FROM asset a WHERE {AllowedAsset} ORDER BY a.id", ct, ("actor", actor.Id))),
        ["isAdmin"] = actor.Has("ADMIN")
    };
    private static async Task<JsonNode> GrantsAsync(DbSession db, Actor actor, CancellationToken ct)
    {
        Access.Demand(actor.Has("ADMIN"));
        return Page(await db.QueryAsync("SELECT g.id,g.user_id,g.role_code,g.asset_id,g.granted_at,g.revoked_at,u.display_name,a.name AS asset_name FROM user_role g JOIN app_user u ON u.id=g.user_id LEFT JOIN asset a ON a.id=g.asset_id ORDER BY g.id", ct));
    }
    private static async Task<JsonNode> AccessAssetsAsync(DbSession db, Actor actor, CancellationToken ct)
    {
        Access.Demand(actor.Has("ADMIN"));
        return Page(await db.QueryAsync("SELECT id,code,name,status FROM asset ORDER BY id", ct));
    }

    public static async Task<JsonObject?> AuditSnapshotAsync(DbSession db, string type, long id, CancellationToken ct)
    {
        string? sql = type switch
        {
            "assets" => "SELECT id,status,owner_id,security_officer_id FROM asset WHERE id=@id",
            "scopes" => "SELECT id,asset_id,version_no,status,confirmed_by,decided_by,effective_from,effective_until FROM asset_scope_revision WHERE id=@id",
            "findings" => "SELECT id,status,final_severity,severity_reason,assessed_by,decision_by,decision_reason FROM finding WHERE id=@id",
            "tickets" => "SELECT t.id,t.assignee_id,t.due_at,t.progress_percent,t.root_cause_masked,t.remediation_action_masked,t.evidence_summary_masked,t.evidence_sha256,t.updated_by,t.remediated_at,f.status FROM remediation_ticket t JOIN finding f ON f.id=t.finding_id WHERE t.id=@id",
            "plans" => "SELECT id,status,scope_id,zap_profile_id,operator_id FROM scan_plan WHERE id=@id",
            "runs" => "SELECT id,scan_status,report_status,verification_outcome,coverage_confirmed,fingerprint_present FROM scan_run WHERE id=@id",
            "profiles" => "SELECT id,status,version_no,active_scan_enabled,approved_by FROM zap_scan_profile WHERE id=@id",
            "users" => "SELECT id,display_name,account_kind,is_active FROM app_user WHERE id=@id",
            "access-grants" => "SELECT id,user_id,role_code,asset_id,revoked_at FROM user_role WHERE id=@id",
            _ => null
        };
        JsonObject? snapshot = sql is null ? null : (await db.QueryAsync(sql, ct, ("id", id))).FirstOrDefault();
        if (type == "scopes" && snapshot is not null) { await AddScopeChildrenAsync(db, snapshot, ct); }
        return snapshot;
    }

    public static async Task<byte[]> ExportAsync(DbSession db, Actor actor, CancellationToken ct)
    {
        List<JsonObject> rows = await db.QueryAsync($"SELECT f.code,a.code AS asset_code,f.title_masked,f.affected_url_masked,f.final_severity,f.status FROM finding f JOIN asset a ON a.id=f.asset_id WHERE {AllowedFinding} ORDER BY f.id LIMIT 50001", ct, ("actor", actor.Id));
        Fields.Require(rows.Count <= 50000, "Báo cáo vượt 50.000 dòng. Hãy thu hẹp phạm vi trước khi xuất.");
        StringBuilder csv = new("Mã phát hiện,Mã tài sản,Phát hiện,URL đã che,Mức độ cuối cùng,Trạng thái\r\n");
        foreach (JsonObject row in rows) { csv.AppendLine(string.Join(',', ExportKeys.Select(key => CsvCell(row.Optional(key) ?? "")))); }
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
    }
    public static string CsvCell(string value)
    {
        string safe = value.TrimStart();
        if (safe.Length > 0 && ("=+-@".Contains(safe[0], StringComparison.Ordinal) || value.StartsWith('\t') || value.StartsWith('\r'))) { value = "'" + value; }
        return "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }
    private static async Task<JsonNode> CreateUserAsync(DbSession db, Actor actor, JsonObject input, CancellationToken ct)
    {
        Access.Demand(actor.Has("ADMIN"));
        string kind = input.Text("accountKind", "Human");
        Fields.Require(kind is "Human" or "Service", "Loại tài khoản không hợp lệ.");
        string? hash = kind == "Service" ? null : Credentials.Hash(input.Text("password"));
        return await db.OneAsync("INSERT INTO app_user(username,display_name,account_kind,password_hash) VALUES(@username,@name,@kind,@hash) RETURNING id,username,display_name,account_kind,is_active,xmin::text::bigint AS version", ct,
            ("username", input.Text("username").ToLowerInvariant()), ("name", input.Text("displayName")), ("kind", kind), ("hash", hash));
    }
    private static async Task<JsonNode> UpdateUserAsync(DbSession db, Actor actor, long id, JsonObject input, string? etag, CancellationToken ct)
    {
        Access.Demand(actor.Has("ADMIN"));
        JsonObject user = await db.OneAsync("SELECT id,account_kind,xmin::text::bigint AS version FROM app_user WHERE id=@id FOR UPDATE", ct, ("id", id));
        Version(user, etag);
        Fields.Require(id != actor.Id || input.Flag("isActive", true), "Không được tự khóa tài khoản đang đăng nhập.");
        string? hash = input.Optional("password") is { Length: > 0 } password ? Credentials.Hash(password) : null;
        Fields.Require(hash is null || user.Text("accountKind") == "Human", "Không đặt mật khẩu cho tài khoản dịch vụ.");
        return await db.OneAsync("UPDATE app_user SET display_name=coalesce(@name,display_name),is_active=coalesce(@active::boolean,is_active),password_hash=coalesce(@hash,password_hash) WHERE id=@id RETURNING id,username,display_name,account_kind,is_active,xmin::text::bigint AS version", ct,
            ("id", id), ("name", input.Optional("displayName")), ("active", input["isActive"]?.GetValue<bool>()), ("hash", hash));
    }
    private static async Task<JsonNode> GrantAsync(DbSession db, Actor actor, JsonObject input, CancellationToken ct)
    {
        Access.Demand(actor.Has("ADMIN"));
        string role = input.Text("roleCode");
        Fields.Require(role is "ADMIN" or "SECURITY" or "SCAN_OPERATOR" or "REMEDIATOR" or "AUDITOR", "Vai trò không hợp lệ.");
        long? asset = input.NullableNumber("assetId");
        Fields.Require(role != "ADMIN" || asset is null, "ADMIN chỉ quản lý quyền toàn hệ thống.");
        JsonObject user = await db.OneAsync("SELECT account_kind FROM app_user WHERE id=@id AND is_active", ct, ("id", input.Number("userId")));
        Fields.Require(user.Text("accountKind") == "Human", "Tài khoản dịch vụ không nhận quyền người dùng.");
        return await db.OneAsync("INSERT INTO user_role(user_id,role_code,asset_id,granted_by) VALUES(@user,@role,@asset,@actor) RETURNING id,user_id,role_code,asset_id", ct,
            ("user", input.Number("userId")), ("role", role), ("asset", asset), ("actor", actor.Id));
    }
    private static async Task<JsonNode> RevokeAsync(DbSession db, Actor actor, long id, CancellationToken ct)
    {
        Access.Demand(actor.Has("ADMIN"));
        JsonObject grant = await db.OneAsync("SELECT id,user_id,role_code FROM user_role WHERE id=@id AND revoked_at IS NULL FOR UPDATE", ct, ("id", id));
        Fields.Require(!(grant.Number("userId") == actor.Id && grant.Text("roleCode") == "ADMIN"), "Không được tự thu hồi vai trò ADMIN đang sử dụng.");
        return await db.OneAsync("UPDATE user_role SET revoked_by=@actor,revoked_at=now() WHERE id=@id RETURNING id,revoked_at", ct, ("actor", actor.Id), ("id", id));
    }
    private static async Task<JsonNode> CreateAssetAsync(DbSession db, Actor actor, JsonObject input, CancellationToken ct)
    {
        long owner = input.NullableNumber("ownerId") ?? actor.Id;
        Access.Demand(owner == actor.Id || actor.Eligible("SECURITY"));
        JsonObject ownerAccount = await db.OneAsync("SELECT account_kind FROM app_user WHERE id=@id AND is_active FOR SHARE", ct, ("id", owner));
        Fields.Require(ownerAccount.Text("accountKind") == "Human", "Chủ tài sản phải là tài khoản người dùng đang hoạt động.");
        await RequireEligibleAsync(db, input.Number("securityOfficerId"), "SECURITY", null, ct);
        Uri url = ScopeRules.SafeUrl(input.Text("baseUrl"));
        Fields.Require(url.Query.Length == 0, "URL tài sản không được chứa query.");
        return await db.OneAsync("INSERT INTO asset(code,name,base_url,environment,owner_id,security_officer_id,criticality,data_classification,internet_exposed,created_by) VALUES(@code,@name,@url,@environment,@owner,@security,@criticality,@classification,@exposed,@actor) RETURNING id,code,name,status,xmin::text::bigint AS version", ct,
            ("code", input.Text("code")), ("name", input.Text("name")), ("url", url.AbsoluteUri), ("environment", input.Text("environment")),
            ("owner", owner), ("security", input.Number("securityOfficerId")), ("criticality", input.Text("criticality")), ("classification", input.Text("dataClassification", "Internal")), ("exposed", input.Flag("internetExposed")), ("actor", actor.Id));
    }
    private static async Task<JsonNode> UpdateAssetAsync(DbSession db, Actor actor, long id, JsonObject input, string? etag, CancellationToken ct)
    {
        JsonObject asset = await RequireAssetAsync(db, actor, id, ct);
        Access.Demand(Access.IsSecurity(actor, asset) || asset.Number("ownerId") == actor.Id);
        Version(asset, etag);
        Fields.Require(asset.Text("status") == "Draft", "Thay đổi thông tin tài sản sau phê duyệt cần quy trình phạm vi mới.", 409, "STATE");
        Uri url = ScopeRules.SafeUrl(input.Text("baseUrl", asset.Text("baseUrl")));
        Fields.Require(url.Query.Length == 0, "URL tài sản không được chứa query.");
        return await db.OneAsync("UPDATE asset a SET name=@name,base_url=@url,environment=@environment,criticality=@criticality,data_classification=@classification,internet_exposed=@exposed,updated_at=now() WHERE a.id=@id RETURNING " + AssetColumns, ct,
            ("id", id), ("name", input.Text("name", asset.Text("name"))), ("url", url.AbsoluteUri), ("environment", input.Text("environment", asset.Text("environment"))),
            ("criticality", input.Text("criticality", asset.Text("criticality"))), ("classification", input.Text("dataClassification", asset.Text("dataClassification"))), ("exposed", input.Flag("internetExposed", asset.Flag("internetExposed"))));
    }
    private static async Task<JsonNode> AssetDetailAsync(DbSession db, Actor actor, long id, CancellationToken ct)
    {
        JsonObject asset = await RequireAssetAsync(db, actor, id, ct);
        List<JsonObject> scopes = await db.QueryAsync("SELECT " + ScopeColumns + " FROM asset_scope_revision s WHERE asset_id=@id ORDER BY version_no DESC", ct, ("id", id));
        foreach (JsonObject scope in scopes) { await AddScopeChildrenAsync(db, scope, ct); }
        asset["scopes"] = new JsonArray(scopes.Select(s => (JsonNode)s).ToArray());
        return asset;
    }
    private static async Task<JsonNode> TransitionAssetAsync(DbSession db, Actor actor, long id, JsonObject input, string? etag, CancellationToken ct)
    {
        JsonObject asset = await RequireAssetAsync(db, actor, id, ct, true);
        Version(asset, etag); _ = input.Text("reason");
        string to = input.Text("toStatus");
        Fields.Require((asset.Text("status"), to) is ("Active", "Suspended") or ("Suspended", "Active") or ("Active" or "Suspended", "Retired"), "Chuyển trạng thái tài sản không hợp lệ.", 409, "STATE");
        if (to is "Suspended" or "Retired")
        {
            await db.ExecuteAsync("UPDATE scan_plan SET status=@status WHERE scope_id IN(SELECT id FROM asset_scope_revision WHERE asset_id=@id) AND status IN('Draft','Active','Paused')", ct, ("id", id), ("status", to == "Retired" ? "Cancelled" : "Paused"));
            await db.ExecuteAsync("UPDATE scan_run SET cancel_requested_at=now() WHERE plan_id IN(SELECT p.id FROM scan_plan p JOIN asset_scope_revision s ON s.id=p.scope_id WHERE s.asset_id=@id) AND scan_status IN('Queued','Running')", ct, ("id", id));
        }
        return await db.OneAsync("UPDATE asset SET status=@status,updated_at=now() WHERE id=@id RETURNING id,status,xmin::text::bigint AS version", ct, ("status", to), ("id", id));
    }
    private static async Task AddScopeChildrenAsync(DbSession db, JsonObject scope, CancellationToken ct)
    {
        long id = scope.Number("id");
        scope["rules"] = Page(await db.QueryAsync("SELECT id,effect,target_type,target_value,match_mode FROM asset_scope_rule WHERE scope_id=@id ORDER BY id", ct, ("id", id)))["items"]!.DeepClone();
        scope["windows"] = Page(await db.QueryAsync("SELECT id,weekday_iso,start_time,end_time,ends_next_day,timezone FROM asset_scan_window WHERE scope_id=@id ORDER BY id", ct, ("id", id)))["items"]!.DeepClone();
    }
    private static async Task<JsonNode> AssignAssetAsync(DbSession db, Actor actor, long id, JsonObject input, string? etag, CancellationToken ct)
    {
        JsonObject asset = await RequireAssetAsync(db, actor, id, ct, true); Version(asset, etag);
        Fields.Require(asset.Text("status") != "Retired", "Tài sản đã ngừng sử dụng.");
        _ = input.Text("reason");
        JsonObject owner = await db.OneAsync("SELECT account_kind FROM app_user WHERE id=@id AND is_active FOR SHARE", ct, ("id", input.Number("ownerId")));
        Fields.Require(owner.Text("accountKind") == "Human", "Chủ tài sản phải là người dùng.");
        await RequireEligibleAsync(db, input.Number("securityOfficerId"), "SECURITY", id, ct);
        return await db.OneAsync("UPDATE asset a SET owner_id=@owner,security_officer_id=@security,updated_at=now() WHERE id=@id RETURNING " + AssetColumns, ct,
            ("id", id), ("owner", input.Number("ownerId")), ("security", input.Number("securityOfficerId")));
    }
    public static async Task<List<ScopeRule>> RulesAsync(DbSession db, long scope, CancellationToken ct) => (await db.QueryAsync("SELECT effect,target_type,target_value,match_mode FROM asset_scope_rule WHERE scope_id=@id", ct, ("id", scope)))
        .Select(r => new ScopeRule(r.Text("effect"), r.Text("targetType"), r.Text("targetValue"), r.Text("matchMode"))).ToList();
    public static async Task<List<ScanWindow>> WindowsAsync(DbSession db, long scope, CancellationToken ct) => (await db.QueryAsync("SELECT weekday_iso,start_time,end_time,ends_next_day,timezone FROM asset_scan_window WHERE scope_id=@id", ct, ("id", scope)))
        .Select(r => new ScanWindow((int)r.Number("weekdayIso"), TimeSpan.Parse(r.Text("startTime"), CultureInfo.InvariantCulture), TimeSpan.Parse(r.Text("endTime"), CultureInfo.InvariantCulture), r.Flag("endsNextDay"), r.Text("timezone"))).ToList();
    private static async Task<JsonNode> SaveScopeAsync(DbSession db, Actor actor, long assetId, long? id, JsonObject input, string? etag, CancellationToken ct)
    {
        JsonObject asset = await RequireAssetAsync(db, actor, assetId, ct);
        Access.Demand(Access.IsSecurity(actor, asset) || asset.Number("ownerId") == actor.Id);
        Fields.Require(asset.Text("status") is "Draft" or "Active" or "Suspended", "Tài sản không cho phép sửa phạm vi.");
        List<ScopeRule> rules = input.Array("rules").Select(n => n!.AsObject()).Select(r => new ScopeRule(r.Text("effect"), r.Text("targetType"), r.Text("targetValue"), r.Text("matchMode"))).ToList();
        Fields.Require(rules.Count is > 0 and <= 100 && rules.Any(r => r.Effect == "Include"), "Cần ít nhất một luật Include, tối đa 100 luật.");
        foreach (ScopeRule rule in rules)
        {
            Fields.Require(rule.Effect is "Include" or "Exclude" && rule.TargetType is "Domain" or "Subdomain" or "URL" && rule.MatchMode is "Exact" or "Prefix", "Luật phạm vi không hợp lệ.");
            if (rule.TargetType == "URL") { Fields.Require(ScopeRules.SafeUrl(rule.TargetValue).Query.Length == 0, "URL phạm vi không được chứa query."); }
            else { Fields.Require(Uri.CheckHostName(rule.TargetValue) != UriHostNameType.Unknown && rule.MatchMode == "Exact", "Domain phạm vi không hợp lệ."); }
        }
        Fields.Require(ScopeRules.Contains(rules, asset.Text("baseUrl")), "URL tài sản phải nằm trong phạm vi cho phép.");
        JsonArray windows = input.Array("windows");
        Fields.Require(windows.Count is > 0 and <= 100, "Cần khung giờ quét.");
        foreach (JsonNode? node in windows) { _ = TimeZoneInfo.FindSystemTimeZoneById(node!.AsObject().Text("timezone")); }
        JsonObject scope;
        if (id is null)
        {
            scope = await db.OneAsync("INSERT INTO asset_scope_revision(asset_id,version_no,change_reason,created_by) SELECT @asset,coalesce(max(version_no),0)+1,@reason,@actor FROM asset_scope_revision WHERE asset_id=@asset RETURNING id,asset_id,version_no,status,xmin::text::bigint AS version", ct,
                ("asset", assetId), ("reason", input.Text("changeReason")), ("actor", actor.Id));
        }
        else
        {
            scope = await db.OneAsync("SELECT " + ScopeColumns + " FROM asset_scope_revision s WHERE s.id=@id AND s.asset_id=@asset FOR UPDATE", ct, ("id", id), ("asset", assetId));
            Version(scope, etag); Fields.Require(scope.Text("status") == "Draft", "Chỉ sửa phạm vi nháp.", 409, "STATE");
            await db.ExecuteAsync("DELETE FROM asset_scope_rule WHERE scope_id=@id; DELETE FROM asset_scan_window WHERE scope_id=@id; UPDATE asset_scope_revision SET change_reason=@reason WHERE id=@id", ct, ("id", id), ("reason", input.Text("changeReason")));
        }
        foreach (ScopeRule rule in rules)
            await db.ExecuteAsync("INSERT INTO asset_scope_rule(scope_id,effect,target_type,target_value,match_mode) VALUES(@scope,@effect,@type,@value,@mode)", ct, ("scope", scope.Number("id")), ("effect", rule.Effect), ("type", rule.TargetType), ("value", rule.TargetValue), ("mode", rule.MatchMode));
        foreach (JsonNode? node in windows)
        {
            JsonObject window = node!.AsObject();
            await db.ExecuteAsync("INSERT INTO asset_scan_window(scope_id,weekday_iso,start_time,end_time,ends_next_day,timezone) VALUES(@scope,@weekday,@start::time,@end::time,@next,@timezone)", ct,
                ("scope", scope.Number("id")), ("weekday", window.Number("weekdayIso")), ("start", window.Text("startTime")), ("end", window.Text("endTime")), ("next", window.Flag("endsNextDay")), ("timezone", window.Text("timezone")));
        }
        return await db.OneAsync("SELECT " + ScopeColumns + " FROM asset_scope_revision s WHERE s.id=@id", ct, ("id", scope.Number("id")));
    }
    private static async Task<JsonNode> ScopeActionAsync(DbSession db, Actor actor, long assetId, long id, string action, JsonObject input, string? etag, CancellationToken ct)
    {
        JsonObject asset = await RequireAssetAsync(db, actor, assetId, ct);
        JsonObject scope = await db.OneAsync("SELECT " + ScopeColumns + " FROM asset_scope_revision s WHERE s.id=@id AND asset_id=@asset FOR UPDATE", ct, ("id", id), ("asset", assetId));
        Version(scope, etag);
        if (action == "end-effect")
        {
            Access.Demand(Access.IsSecurity(actor, asset));
            Fields.Require(scope.Text("status") == "Approved" && scope.NullableDate("effectiveUntil") is null, "Chỉ đặt ngày kết thúc một lần cho phạm vi đã duyệt chưa có hạn.");
            DateTime until = input.Date("effectiveUntil"); _ = input.Text("reason");
            Fields.Require(until > DateTime.UtcNow && until > scope.Date("effectiveFrom"), "Ngày kết thúc phải ở tương lai và sau ngày hiệu lực.");
            await db.ExecuteAsync("UPDATE asset_scope_revision SET effective_until=@until WHERE id=@id", ct, ("id", id), ("until", until));
            return await db.OneAsync("SELECT " + ScopeColumns + " FROM asset_scope_revision s WHERE s.id=@id", ct, ("id", id));
        }
        if (action == "confirm")
        {
            Access.Demand(asset.Number("ownerId") == actor.Id); Fields.Require(scope.Text("status") == "Draft", "Phạm vi không ở trạng thái nháp.", 409, "STATE");
            await db.ExecuteAsync("UPDATE asset_scope_revision SET status='Pending Approval',confirmed_by=@actor,confirmed_at=now() WHERE id=@id", ct, ("id", id), ("actor", actor.Id));
            await db.ExecuteAsync("UPDATE asset SET status='Pending Approval',updated_at=now() WHERE id=@id AND status='Draft'", ct, ("id", assetId));
        }
        else if (action == "decision")
        {
            Access.Demand(Access.IsSecurity(actor, asset)); Fields.Require(scope.Text("status") == "Pending Approval", "Phạm vi chưa được chủ tài sản xác nhận.", 409, "STATE");
            Fields.Require(scope.Number("confirmedBy") != actor.Id, "Người xác nhận không tự phê duyệt phạm vi.");
            string decision = input.Text("decision"); Fields.Require(decision is "Approved" or "Rejected", "Quyết định không hợp lệ.");
            DateTime from = input.NullableDate("effectiveFrom") ?? DateTime.UtcNow;
            DateTime? until = input.NullableDate("effectiveUntil");
            await db.ExecuteAsync("UPDATE asset_scope_revision SET status=@status,decided_by=@actor,decided_at=now(),decision_reason=@reason,effective_from=@from,effective_until=@until WHERE id=@id", ct,
                ("id", id), ("status", decision), ("actor", actor.Id), ("reason", Protection.Mask(input.Text("reason"))), ("from", from), ("until", until));
            await db.ExecuteAsync("UPDATE asset SET status=@status,updated_at=now() WHERE id=@id AND status='Pending Approval'", ct, ("id", assetId), ("status", decision == "Approved" ? "Active" : "Draft"));
        }
        else { throw new BusinessException(404, "NOT_FOUND", "Thao tác phạm vi không tồn tại."); }
        return await db.OneAsync("SELECT " + ScopeColumns + " FROM asset_scope_revision s WHERE s.id=@id", ct, ("id", id));
    }
}
