using System.Security.Cryptography;
using System.Text.Json.Nodes;

namespace Htqlhdk.Core;

public sealed partial class CoreService
{
    public async Task<JsonNode> EvidenceAsync(DbSession db, Actor actor, long id, byte[] bytes, string summary, string? etag, CancellationToken ct)
    {
        Fields.Require(bytes.Length is > 0 and <= 10485760, "Bằng chứng phải có dung lượng từ 1 byte đến 10 MB.", 413, "UPLOAD_SIZE");
        Fields.Require(summary.Trim().Length is >= 3 and <= 8000, "Cần mô tả bằng chứng.");
        JsonObject ticket = (await TicketDetailAsync(db, actor, id, ct)).AsObject(); Version(ticket, etag);
        JsonObject finding = ticket["finding"]!.AsObject();
        Access.Demand(ticket.Number("assigneeId") == actor.Id && actor.Has("REMEDIATOR", finding.Number("assetId")));
        Fields.Require(finding.Text("status") is "Assigned" or "In Progress", "Chỉ tải bằng chứng khi đang khắc phục.", 409, "STATE");
        string key = await protection.StoreAsync(bytes, ct);
        try
        {
            await db.ExecuteAsync("UPDATE remediation_ticket SET evidence_object_key=@key,evidence_sha256=@hash,evidence_key_ref='local-aes-v1',evidence_summary_masked=@summary,updated_by=@actor,updated_at=now() WHERE id=@id", ct,
                ("id", id), ("key", key), ("hash", Convert.ToHexStringLower(SHA256.HashData(bytes))), ("summary", Protection.Mask(summary)), ("actor", actor.Id));
        }
        catch { protection.Remove(key); throw; }
        return await TicketDetailAsync(db, actor, id, ct);
    }

    public async Task<byte[]> ReadEvidenceAsync(DbSession db, Actor actor, long id, CancellationToken ct)
    {
        JsonObject ticket = (await TicketDetailAsync(db, actor, id, ct)).AsObject();
        await RequireAssetAsync(db, actor, ticket["finding"]!.AsObject().Number("assetId"), ct, true);
        JsonObject stored = await db.OneAsync("SELECT evidence_object_key FROM remediation_ticket WHERE id=@id AND evidence_object_key IS NOT NULL", ct, ("id", id));
        return await protection.ReadAsync(stored.Text("evidenceObjectKey"), ct);
    }
}
