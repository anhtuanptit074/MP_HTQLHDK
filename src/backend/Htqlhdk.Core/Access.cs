using System.Text.Json.Nodes;

namespace Htqlhdk.Core;

public sealed record Grant(long Id, string Role, long? AssetId);
public sealed record Actor(long Id, string Name, string Kind, IReadOnlyList<Grant> Grants)
{
    public bool Has(string role, long? assetId = null) => Grants.Any(g => g.Role == role && g.AssetId == assetId);
    public bool Eligible(string role) => Grants.Any(g => g.Role == role);

    public static async Task<Actor> LoadAsync(DbSession db, long id, CancellationToken ct, bool service = false)
    {
        JsonObject user = await db.OneAsync("SELECT id,display_name,account_kind FROM app_user WHERE id=@id AND is_active FOR SHARE", ct, ("id", id));
        Fields.Require(service || user.Text("accountKind") == "Human", "Tài khoản dịch vụ không được đăng nhập.", 403, "FORBIDDEN");
        List<JsonObject> rows = await db.QueryAsync("SELECT id,role_code,asset_id FROM user_role WHERE user_id=@id AND revoked_at IS NULL FOR SHARE", ct, ("id", id));
        return new(id, user.Text("displayName"), user.Text("accountKind"), rows.Select(r => new Grant(r.Number("id"), r.Text("roleCode"), r.NullableNumber("assetId"))).ToList());
    }
}

public static class Access
{
    public static bool CanReadAsset(Actor actor, JsonObject asset, bool assigned)
    {
        long id = asset.Number("id");
        return actor.Kind == "Service" && assigned || asset.Number("ownerId") == actor.Id ||
            (asset.Number("securityOfficerId") == actor.Id && actor.Has("SECURITY", id)) ||
            actor.Has("AUDITOR", id) || (assigned && (actor.Has("SCAN_OPERATOR", id) || actor.Has("REMEDIATOR", id)));
    }
    public static bool IsSecurity(Actor actor, JsonObject asset) => asset.Number("securityOfficerId") == actor.Id && actor.Has("SECURITY", asset.Number("id"));
    public static void Demand(bool allowed) => Fields.Require(allowed, "Không đủ quyền hoặc ngoài phạm vi được phép.", 403, "FORBIDDEN");

    public static async Task AuditAsync(DbSession db, Actor? actor, string action, string objectType, string objectId, Guid correlation,
        CancellationToken ct, string outcome = "Success", JsonObject? before = null, JsonObject? after = null, string? reason = null)
    {
        string category = action.StartsWith("access.", StringComparison.Ordinal) || action.StartsWith("user.", StringComparison.Ordinal) ? "Permission" :
            action.StartsWith("auth.", StringComparison.Ordinal) ? "Security" : "Business";
        await db.ExecuteAsync("INSERT INTO audit_log(actor_id,actor_kind,category,action,object_type,object_id,outcome,before_masked,after_masked,reason_masked,correlation_id) VALUES(@actor,@kind,@category,@action,@type,@id,@outcome,@before::jsonb,@after::jsonb,@reason,@correlation)", ct,
            ("actor", actor?.Id), ("kind", actor?.Kind ?? "Anonymous"), ("category", category), ("action", action), ("type", objectType), ("id", objectId),
            ("outcome", outcome), ("before", before?.ToJsonString()), ("after", after?.ToJsonString()), ("reason", reason is null ? null : Protection.Mask(reason)), ("correlation", correlation));
    }
}
