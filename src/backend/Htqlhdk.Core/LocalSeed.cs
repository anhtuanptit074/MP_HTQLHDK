using System.Text;
using System.Text.Json.Nodes;

namespace Htqlhdk.Core;

public static class LocalSeed
{
    public static async Task RunAsync(CoreDatabase database, CoreService service, CancellationToken ct)
    {
        await using DbSession db = await database.OpenAsync(ct);
        Dictionary<string, long> users = [];
        List<string> credentials = [];
        foreach ((string username, string name, string kind) in new[] {
            ("admin", "Quản trị quyền", "Human"), ("owner", "Chủ tài sản", "Human"), ("security", "Chuyên viên ATTT", "Human"),
            ("operator", "Vận hành quét", "Human"), ("remediator", "Kỹ thuật khắc phục", "Human"), ("auditor", "Kiểm toán viên", "Human"),
            ("owner2", "Chủ tài sản thứ hai", "Human"), ("svc-zap", "Dịch vụ ZAP", "Service") })
        {
            List<JsonObject> existing = await db.QueryAsync("SELECT id FROM app_user WHERE username=@username", ct, ("username", username));
            if (existing.Count > 0) { users[username] = existing[0].Number("id"); continue; }
            string? password = kind == "Human" ? Credentials.Generate() : null;
            JsonObject created = await db.OneAsync("INSERT INTO app_user(username,display_name,account_kind,password_hash) VALUES(@username,@name,@kind,@hash) RETURNING id", ct,
                ("username", username), ("name", name), ("kind", kind), ("hash", password is null ? null : Credentials.Hash(password)));
            users[username] = created.Number("id");
            if (password is not null) { credentials.Add(username + "\t" + password + "\t" + name); }
        }
        foreach ((string name, string role) in new[] { ("admin", "ADMIN"), ("security", "SECURITY"), ("operator", "SCAN_OPERATOR"), ("remediator", "REMEDIATOR") })
        {
            await db.ExecuteAsync("INSERT INTO user_role(user_id,role_code,granted_by) SELECT @user,@role,@admin WHERE NOT EXISTS(SELECT 1 FROM user_role WHERE user_id=@user AND role_code=@role AND asset_id IS NULL) ON CONFLICT DO NOTHING", ct,
                ("user", users[name]), ("role", role), ("admin", users["admin"]));
        }
        if ((await db.QueryAsync("SELECT id FROM asset LIMIT 1", ct)).Count == 0)
        {
            foreach ((string code, string name, string url, string owner) in new[] {
                ("SHOP-DEMO", "Website bán hàng mẫu", "https://shop.example.invalid/", "owner"),
                ("PORTAL-DEMO", "Cổng thông tin mẫu", "https://portal.example.invalid/", "owner2") })
            {
                JsonObject asset = await db.OneAsync("INSERT INTO asset(code,name,base_url,environment,owner_id,security_officer_id,criticality,created_by) VALUES(@code,@name,@url,'Test',@owner,@security,'High',@owner) RETURNING id", ct,
                    ("code", code), ("name", name), ("url", url), ("owner", users[owner]), ("security", users["security"]));
                long assetId = asset.Number("id");
                foreach ((string username, string role) in new[] { ("security", "SECURITY"), ("operator", "SCAN_OPERATOR"), ("remediator", "REMEDIATOR"), ("auditor", "AUDITOR") })
                    await db.ExecuteAsync("INSERT INTO user_role(user_id,role_code,asset_id,granted_by) VALUES(@user,@role,@asset,@admin)", ct, ("user", users[username]), ("role", role), ("asset", assetId), ("admin", users["admin"]));
                JsonObject scope = await db.OneAsync("INSERT INTO asset_scope_revision(asset_id,version_no,change_reason,created_by) VALUES(@asset,1,'Phạm vi mẫu local',@owner) RETURNING id", ct, ("asset", assetId), ("owner", users[owner]));
                await db.ExecuteAsync("INSERT INTO asset_scope_rule(scope_id,effect,target_type,target_value,match_mode) VALUES(@scope,'Include','Domain',@domain,'Exact')", ct, ("scope", scope.Number("id")), ("domain", new Uri(url).Host));
                for (int weekday = 1; weekday <= 7; weekday++)
                    await db.ExecuteAsync("INSERT INTO asset_scan_window(scope_id,weekday_iso,start_time,end_time,ends_next_day,timezone) VALUES(@scope,@day,'00:00','00:00',true,'Asia/Bangkok')", ct, ("scope", scope.Number("id")), ("day", weekday));
                await db.ExecuteAsync("UPDATE asset_scope_revision SET status='Pending Approval',confirmed_by=@owner,confirmed_at=now() WHERE id=@scope; UPDATE asset_scope_revision SET status='Approved',decided_by=@security,decided_at=now(),decision_reason='Phạm vi giả chỉ dùng local',effective_from=now()-interval '10 days' WHERE id=@scope; UPDATE asset SET status='Active' WHERE id=@asset", ct,
                    ("scope", scope.Number("id")), ("asset", assetId), ("owner", users[owner]), ("security", users["security"]));
                JsonObject profile = await db.OneAsync("INSERT INTO zap_scan_profile(code,name,version_no,status,service_user_id,created_by,approved_by,approved_at,max_duration_minutes) VALUES(@code,'Passive scan mẫu',1,'Active',@service,@operator,@security,now(),10) RETURNING id", ct,
                    ("code", "PASSIVE-" + code), ("service", users["svc-zap"]), ("operator", users["operator"]), ("security", users["security"]));
                DateTime when = DateTime.UtcNow.Date.AddHours(1).AddDays(-1);
                JsonObject plan = await db.OneAsync("INSERT INTO scan_plan(code,name,plan_kind,scope_id,zap_profile_id,operator_id,status,recurrence_rule,timezone,effective_from,created_by,approved_by,approved_at) VALUES(@code,'Quét định kỳ mẫu','Periodic',@scope,@profile,@operator,'Active','FREQ=DAILY','Asia/Bangkok',@from,@security,@security,now()) RETURNING id", ct,
                    ("code", "PLAN-" + code), ("scope", scope.Number("id")), ("profile", profile.Number("id")), ("operator", users["operator"]), ("from", when.AddDays(-1)), ("security", users["security"]));
                JsonObject run = await db.OneAsync("INSERT INTO scan_run(plan_id,code,scheduled_for,scan_status,started_at,finished_at) VALUES(@plan,@code,@when,'Completed',@when,@finished) RETURNING id,xmin::text::bigint AS version", ct,
                    ("plan", plan.Number("id")), ("code", "RUN-DEMO-" + code), ("when", when), ("finished", when.AddMinutes(5)));
                byte[] report = Encoding.UTF8.GetBytes(new JsonObject
                {
                    ["@version"] = "2.17.0", ["site"] = new JsonArray(new JsonObject
                    {
                        ["@name"] = url, ["alerts"] = new JsonArray(new JsonObject {
                            ["pluginid"] = "10020", ["name"] = "Thiếu header chống clickjacking", ["riskcode"] = "2", ["confidence"] = "2",
                            ["desc"] = "Phản hồi chưa có Content-Security-Policy frame-ancestors.", ["solution"] = "Thiết lập Content-Security-Policy phù hợp.",
                            ["instances"] = new JsonArray(new JsonObject { ["uri"] = url, ["method"] = "GET", ["param"] = "" })
                        })
                    })
                }.ToJsonString());
                Actor actor = await Actor.LoadAsync(db, users["operator"], ct);
                await service.ImportAsync(db, actor, run.Number("id"), report, "JSON", "\"" + run.Number("version") + "\"", ct);
            }
            await Access.AuditAsync(db, await Actor.LoadAsync(db, users["admin"], ct), "user.local-seed", "users", "0", Guid.NewGuid(), ct);
        }
        await db.CommitAsync(ct);
        if (credentials.Count > 0)
        {
            string file = Path.Combine(CoreConfiguration.Root(), ".codex_tmp", "local-accounts.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            await File.AppendAllLinesAsync(file, credentials, ct);
        }
        Console.WriteLine("PASS: core13 local accounts and sample data prepared. Credentials are in .codex_tmp/local-accounts.txt.");
    }
}
