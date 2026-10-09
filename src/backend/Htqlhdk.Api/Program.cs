using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.RateLimiting;
using Htqlhdk.Api.Middleware;
using Htqlhdk.Core;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Npgsql;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
CoreConfiguration.LoadLocalEnvironment(builder.Environment.IsDevelopment());
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton(_ => NpgsqlDataSource.Create(CoreConfiguration.ConnectionString()));
builder.Services.AddSingleton<CoreDatabase>();
builder.Services.AddSingleton(_ => CoreConfiguration.Storage(builder.Environment.IsDevelopment()));
builder.Services.AddSingleton<CoreService>();
builder.Services.AddAntiforgery(options => { options.HeaderName = "X-CSRF-Token"; options.Cookie.SameSite = SameSiteMode.Strict; });
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(CoreConfiguration.Root(), ".codex_tmp", "session-keys"))).SetApplicationName("HTQLHDK.Core13");
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.Name = "htqlhdk.session"; options.Cookie.HttpOnly = true; options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromHours(8); options.SlidingExpiration = false;
    options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = 401; return Task.CompletedTask; };
    options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = 403; return Task.CompletedTask; };
    options.Events.OnValidatePrincipal = async context =>
    {
        if (!long.TryParse(context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out long id)) { context.RejectPrincipal(); return; }
        await using DbSession db = await context.HttpContext.RequestServices.GetRequiredService<CoreDatabase>().OpenAsync(context.HttpContext.RequestAborted);
        List<JsonObject> rows = await db.QueryAsync("SELECT password_hash FROM app_user WHERE id=@id AND is_active AND account_kind='Human'", context.HttpContext.RequestAborted, ("id", id));
        if (rows.Count == 0 || Stamp(rows[0].Optional("passwordHash") ?? "") != context.Principal?.FindFirstValue("stamp")) { context.RejectPrincipal(); }
    };
});
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options => { options.RejectionStatusCode = 429; options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
    _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 })); });
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options => options.MultipartBodyLengthLimit = 52428800);
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 53477376);
WebApplication app = builder.Build();
if (args.Contains("--seed-local", StringComparer.Ordinal))
{
    if (!app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("HTQLHDK_ENVIRONMENT") != "Local") { throw new InvalidOperationException("Local seeding is unavailable outside Local/Development."); }
    await LocalSeed.RunAsync(app.Services.GetRequiredService<CoreDatabase>(), app.Services.GetRequiredService<CoreService>(), CancellationToken.None);
    return;
}
app.UseMiddleware<CorrelationIdMiddleware>();
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers.CacheControl = "no-store";
    try { await next(context); }
    catch (Exception exception) when (exception is BusinessException or NpgsqlException or BadHttpRequestException or IOException or JsonException or FormatException or InvalidOperationException or ArgumentException or TimeZoneNotFoundException or InvalidTimeZoneException or AntiforgeryValidationException or System.Xml.XmlException)
    {
        bool unavailable = exception is NpgsqlException and not PostgresException || exception is IOException and not BadHttpRequestException;
        int status = unavailable ? 503 : exception is BadHttpRequestException requestError ? requestError.StatusCode : exception is BusinessException business ? business.Status : exception is AntiforgeryValidationException ? 400 : exception is PostgresException pg && pg.SqlState == "23505" ? 409 : 422;
        string code = unavailable ? "SERVICE_UNAVAILABLE" : exception is BusinessException rule ? rule.Code : exception is AntiforgeryValidationException ? "CSRF" : "VALIDATION";
        string message = unavailable ? "Kết nối dịch vụ tạm thời gián đoạn. Vui lòng tải lại sau ít giây." : exception is BusinessException validation ? validation.Message : "Yêu cầu không hợp lệ hoặc xung đột dữ liệu. Hãy kiểm tra và tải lại.";
        if (exception is AntiforgeryValidationException)
        {
            await using DbSession audit = await context.RequestServices.GetRequiredService<CoreDatabase>().OpenAsync(CancellationToken.None);
            await Access.AuditAsync(audit, null, "auth.csrf", "auth", "0", Correlation(context), CancellationToken.None, "Denied", reason: "Yêu cầu không có xác thực chống giả mạo hợp lệ"); await audit.CommitAsync();
        }
        if (!context.Response.HasStarted) { await Results.Problem(statusCode: status, title: message, extensions: new Dictionary<string, object?> { ["code"] = code, ["correlationId"] = Correlation(context), ["retryable"] = unavailable }).ExecuteAsync(context); }
    }
});
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/", () => Results.Ok(new { service = "HTQLHDK API", status = "core13-ready", stage = 11, model = "core13" }));
app.MapGet("/health/live", () => Results.Text("Healthy"));
app.MapGet("/health/ready", async (CoreDatabase database, CancellationToken ct) => { await using DbSession db = await database.OpenAsync(ct); await db.OneAsync("SELECT count(*) AS tables FROM information_schema.tables WHERE table_schema='htqlhdk' AND table_type='BASE TABLE' HAVING count(*)=13", ct); return Results.Text("Healthy"); });
app.MapGet("/api/v1/auth/csrf", (HttpContext context, IAntiforgery antiforgery) => Results.Ok(new { token = antiforgery.GetAndStoreTokens(context).RequestToken }));
app.MapPost("/api/v1/auth/login", async (HttpContext context, CoreDatabase database, IAntiforgery antiforgery) =>
{
    await antiforgery.ValidateRequestAsync(context);
    JsonObject input = await context.Request.ReadFromJsonAsync<JsonObject>(context.RequestAborted) ?? [];
    string username = input.Text("username").ToLowerInvariant(); string password = input.Text("password");
    await using DbSession db = await database.OpenAsync(context.RequestAborted);
    List<JsonObject> users = await db.QueryAsync("SELECT id,display_name,password_hash FROM app_user WHERE username=@username AND is_active AND account_kind='Human' FOR SHARE", context.RequestAborted, ("username", username));
    string hash = users.Count > 0 ? users[0].Optional("passwordHash") ?? Program.DummyHash : Program.DummyHash;
    bool valid = Credentials.Verify(hash, password) && users.Count > 0;
    Actor? actor = valid ? await Actor.LoadAsync(db, users[0].Number("id"), context.RequestAborted) : null;
    await Access.AuditAsync(db, actor, "auth.login", "auth", "0", Correlation(context), context.RequestAborted, valid ? "Success" : "Denied");
    await db.CommitAsync(context.RequestAborted);
    Fields.Require(valid, "Tên đăng nhập hoặc mật khẩu không đúng.", 401, "LOGIN_FAILED");
    ClaimsIdentity identity = new([new Claim(ClaimTypes.NameIdentifier, actor!.Id.ToString(CultureInfo.InvariantCulture)), new Claim(ClaimTypes.Name, actor.Name), new Claim("stamp", Stamp(hash))], CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    return Results.Ok(new { userId = actor.Id, displayName = actor.Name });
}).RequireRateLimiting("login");
app.MapPost("/api/v1/auth/logout", async (HttpContext context, IAntiforgery antiforgery, CoreDatabase database) =>
{
    await antiforgery.ValidateRequestAsync(context);
    await using DbSession db = await database.OpenAsync(context.RequestAborted);
    Actor actor = await Actor.LoadAsync(db, long.Parse(context.User.FindFirstValue(ClaimTypes.NameIdentifier)!, CultureInfo.InvariantCulture), context.RequestAborted);
    await Access.AuditAsync(db, actor, "auth.logout", "auth", "0", Correlation(context), context.RequestAborted); await db.CommitAsync(context.RequestAborted);
    await context.SignOutAsync(); return Results.NoContent();
}).RequireAuthorization();
app.MapMethods("/api/v1/{**route}", ["GET", "POST", "PUT", "PATCH"], async (HttpContext context, string route, CoreDatabase database, CoreService service, IAntiforgery antiforgery) =>
{
    CancellationToken ct = context.RequestAborted;
    string[] path = route.Trim('/').Split('/');
    bool mutate = context.Request.Method != "GET";
    if (mutate) { await antiforgery.ValidateRequestAsync(context); }
    long actorId = long.Parse(context.User.FindFirstValue(ClaimTypes.NameIdentifier)!, CultureInfo.InvariantCulture);
    Actor? actor = null;
    string type = path[0]; string objectId = path.Length > 1 && long.TryParse(path[1], out _) ? path[1] : "0";
    string action = type == "access-grants" ? "access." + (mutate ? "change" : "read") : type == "users" ? "user." + (mutate ? "change" : "read") : type + "." + (path.Length > 2 ? path[^1] : mutate ? "change" : "read");
    bool createsResource = mutate && (path.Length == 1 || path is ["plans", _, "runs"] or ["findings", _, "tickets"] or ["tickets", _, "verification-plans"] or ["assets", _, "scopes"]);
    if (path is ["plans", _, "runs"]) { type = "runs"; }
    if (path is ["findings", _, "tickets"]) { type = "tickets"; }
    if (path is ["tickets", _, "verification-plans"]) { type = "plans"; }
    if (path is ["assets", _, "scopes", ..]) { type = "scopes"; objectId = path.Length > 3 ? path[3] : "0"; }
    if (createsResource) { objectId = "0"; }
    try
    {
        await using DbSession db = await database.OpenAsync(ct);
        actor = await Actor.LoadAsync(db, actorId, ct);
        JsonObject? before = mutate && long.TryParse(objectId, out long existingId) && existingId > 0 ? await CoreService.AuditSnapshotAsync(db, type, existingId, ct) : null;
        string? auditReason = null;
        JsonNode result;
        if (path is ["runs", var runId, "report"] && mutate)
        {
            Fields.Require(context.Request.HasFormContentType, "Cần tải lên báo cáo dạng multipart.");
            IFormCollection form = await context.Request.ReadFormAsync(ct); IFormFile? file = form.Files.GetFile("file");
            Fields.Require(file is not null && file.Length is > 0 and <= 52428800, "Cần báo cáo JSON/XML tối đa 50 MB.");
            using MemoryStream bytes = new(); await file!.CopyToAsync(bytes, ct);
            string format = form["format"].ToString(); Fields.Require(format is "JSON" or "XML", "Định dạng báo cáo không hợp lệ.");
            result = await service.ImportAsync(db, actor, CoreService.ParseId(runId), bytes.ToArray(), format, context.Request.Headers.IfMatch, ct);
        }
        else if (path is ["tickets", var ticketId, "evidence"] && mutate)
        {
            Fields.Require(context.Request.HasFormContentType, "Cần tải lên bằng chứng dạng multipart.");
            IFormCollection form = await context.Request.ReadFormAsync(ct); IFormFile? file = form.Files.GetFile("file");
            Fields.Require(file is not null && file.Length is > 0 and <= 10485760, "Bằng chứng tối đa 10 MB.");
            using MemoryStream bytes = new(); await file!.CopyToAsync(bytes, ct);
            result = await service.EvidenceAsync(db, actor, CoreService.ParseId(ticketId), bytes.ToArray(), form["summary"].ToString(), context.Request.Headers.IfMatch, ct);
            auditReason = form["summary"].ToString();
        }
        else if (path is ["tickets", var evidenceId, "evidence"] && !mutate)
        {
            Fields.Require(context.Request.Headers["X-Access-Reason"].ToString().Length >= 3, "Cần lý do truy cập bằng chứng gốc.");
            byte[] bytes = await service.ReadEvidenceAsync(db, actor, CoreService.ParseId(evidenceId), ct);
            await Access.AuditAsync(db, actor, "tickets.evidence-read", "tickets", evidenceId, Correlation(context), ct, reason: context.Request.Headers["X-Access-Reason"].ToString()); await db.CommitAsync(ct);
            return Results.File(bytes, "application/octet-stream", "bang-chung.bin");
        }
        else if (path is ["runs", var rawId, "raw-report"] && !mutate)
        {
            Fields.Require(context.Request.Headers["X-Access-Reason"].ToString().Length >= 3, "Cần lý do truy cập báo cáo gốc.");
            (byte[] bytes, string format) = await service.RawReportAsync(db, actor, CoreService.ParseId(rawId), ct);
            await Access.AuditAsync(db, actor, "runs.raw-report", "runs", rawId, Correlation(context), ct, reason: context.Request.Headers["X-Access-Reason"].ToString()[..Math.Min(500, context.Request.Headers["X-Access-Reason"].ToString().Length)]); await db.CommitAsync(ct);
            return Results.File(bytes, format == "JSON" ? "application/json" : "application/xml", "zap-report." + format.ToLowerInvariant());
        }
        else if (path is ["reports", "export"] && !mutate)
        {
            byte[] csv = await CoreService.ExportAsync(db, actor, ct);
            await Access.AuditAsync(db, actor, "reports.export", "reports", "0", Correlation(context), ct, after: new JsonObject { ["sizeBytes"] = csv.Length }); await db.CommitAsync(ct);
            return Results.File(csv, "text/csv; charset=utf-8", "bao-cao-phat-hien.csv");
        }
        else
        {
            JsonObject input = mutate && context.Request.ContentLength is > 0 ? await context.Request.ReadFromJsonAsync<JsonObject>(ct) ?? [] : [];
            auditReason = input.Optional("reason") ?? input.Optional("changeReason") ?? input.Optional("severityReason");
            long after = long.TryParse(context.Request.Query["page[after]"], out long cursor) ? Math.Max(0, cursor) : 0;
            int size = int.TryParse(context.Request.Query["page[size]"], out int pageSize) ? Math.Clamp(pageSize, 1, 200) : 100;
            result = await CoreService.DispatchAsync(db, actor, context.Request.Method, path, input, context.Request.Headers.IfMatch, after, size, ct);
            if (createsResource && result is JsonObject created && created.NullableNumber("id") is long id) { objectId = id.ToString(CultureInfo.InvariantCulture); }
        }
        JsonObject snapshot = result is JsonObject resource ? new JsonObject { ["id"] = resource["id"]?.DeepClone(), ["status"] = resource["status"]?.DeepClone(), ["version"] = resource["version"]?.DeepClone() } : [];
        if (mutate && long.TryParse(objectId, out long changedId) && changedId > 0) { snapshot = await CoreService.AuditSnapshotAsync(db, type, changedId, ct) ?? snapshot; }
        if (mutate || path is ["reports"] or ["audit-logs"]) { await Access.AuditAsync(db, actor, action, type, objectId, Correlation(context), ct, before: before, after: snapshot, reason: auditReason); }
        await db.CommitAsync(ct);
        if (result is JsonObject versioned && versioned.NullableNumber("version") is long version) { context.Response.Headers.ETag = "\"" + version.ToString(CultureInfo.InvariantCulture) + "\""; }
        return Results.Json(result);
    }
    catch (Exception exception) when (exception is BusinessException or PostgresException or JsonException or FormatException or InvalidOperationException or ArgumentException or TimeZoneNotFoundException or InvalidTimeZoneException or System.Xml.XmlException)
    {
        await using DbSession audit = await database.OpenAsync(ct);
        if (actor is not null && path is ["runs", var rejectedId, "report"] && exception is not BusinessException { Status: 403 or 404 or 409 or 412 or 428 })
        {
            await CoreService.RejectReportAsync(audit, actor, CoreService.ParseId(rejectedId), exception is BusinessException rejected ? rejected.Code : "REPORT_SCHEMA", ct);
        }
        if (actor is not null) { await Access.AuditAsync(audit, actor, action, type, objectId, Correlation(context), ct, exception is BusinessException { Status: 403 or 404 } ? "Denied" : "Failure", reason: "Yêu cầu bị từ chối hoặc không hợp lệ"); await audit.CommitAsync(ct); }
        throw;
    }
}).RequireAuthorization();
await app.RunAsync();

static Guid Correlation(HttpContext context) => Guid.TryParse(context.Response.Headers["X-Correlation-ID"], out Guid id) ? id : Guid.NewGuid();
static string Stamp(string hash) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(hash)));

internal static partial class Program
{
    internal static readonly string DummyHash = Credentials.Hash("not-a-real-password-1234");
}
