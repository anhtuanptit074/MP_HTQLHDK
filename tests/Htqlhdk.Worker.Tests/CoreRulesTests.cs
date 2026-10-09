using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using Htqlhdk.Core;

namespace Htqlhdk.Worker.Tests;

[TestClass]
public sealed class CoreRulesTests
{
    private static readonly ScopeRule[] Scope = [new("Include", "Domain", "example.invalid", "Exact"), new("Exclude", "URL", "https://example.invalid/admin", "Prefix")];

    [TestMethod]
    public void DatabaseNumericAndTimestampTypesAreReadConsistently()
    {
        JsonObject row = new() { ["short"] = System.Text.Json.JsonSerializer.SerializeToNode((short)2), ["int"] = System.Text.Json.JsonSerializer.SerializeToNode(10), ["long"] = 4L, ["at"] = Utc("2026-10-05T01:00:00Z") };
        Assert.AreEqual(2L, row.Number("short")); Assert.AreEqual(10L, row.Number("int")); Assert.AreEqual(4L, row.Number("long"));
        Assert.AreEqual(Utc("2026-10-05T01:00:00Z"), row.Date("at"));
    }

    [TestMethod]
    public void NullRoleGrantDoesNotReadAnyAsset()
    {
        Actor actor = new(7, "Eligible only", "Human", [new(1, "SECURITY", null), new(2, "AUDITOR", null)]);
        JsonObject asset = new() { ["id"] = 11L, ["ownerId"] = 4L, ["securityOfficerId"] = 7L };
        Assert.IsTrue(actor.Eligible("SECURITY"));
        Assert.IsFalse(Access.CanReadAsset(actor, asset, true));
        Assert.IsFalse(Access.IsSecurity(actor, asset));
    }

    [TestMethod]
    public void AdminHasNoImplicitBusinessReadPermission()
    {
        Actor actor = new(7, "Admin", "Human", [new(1, "ADMIN", null)]);
        JsonObject asset = new() { ["id"] = 11L, ["ownerId"] = 4L, ["securityOfficerId"] = 5L };
        Assert.IsFalse(Access.CanReadAsset(actor, asset, true));
    }

    [TestMethod]
    public void ScopedTaskRolesNeedAssignment()
    {
        Actor actor = new(7, "Operator", "Human", [new(1, "SCAN_OPERATOR", 11)]);
        JsonObject asset = new() { ["id"] = 11L, ["ownerId"] = 4L, ["securityOfficerId"] = 5L };
        Assert.IsFalse(Access.CanReadAsset(actor, asset, false));
        Assert.IsTrue(Access.CanReadAsset(actor, asset, true));
    }

    [TestMethod]
    public void ExclusionWinsAndPrefixHasPathBoundary()
    {
        Assert.IsTrue(ScopeRules.Contains(Scope, "https://example.invalid/administer"));
        Assert.IsFalse(ScopeRules.Contains(Scope, "https://example.invalid/admin/reset"));
        Assert.IsFalse(ScopeRules.Contains(Scope, "https://example.invalid.evil.invalid/"));
        Assert.IsFalse(ScopeRules.Contains(Scope, "https://evil.example.invalid/"));
        Assert.ThrowsExactly<BusinessException>(() => ScopeRules.SafeUrl("https://user:password@example.invalid/"));
    }

    [TestMethod]
    public void SubdomainRuleNeedsDottedBoundary()
    {
        ScopeRule[] sub = [new("Include", "Subdomain", "example.invalid", "Exact")];
        Assert.IsTrue(ScopeRules.Contains(sub, "https://a.example.invalid/"));
        Assert.IsFalse(ScopeRules.Contains(sub, "https://notexample.invalid/"));
        Assert.IsFalse(ScopeRules.Contains(sub, "https://example.invalid/"));
    }

    [TestMethod]
    public void OvernightWindowIncludesPreviousDayAndEndIsExclusive()
    {
        ScanWindow[] windows = [new(1, TimeSpan.FromHours(22), TimeSpan.FromHours(2), true, "Asia/Bangkok")];
        DateTime utc = Utc("2026-10-05T18:00:00Z"); // Tuesday 01:00 locally, Monday's overnight window.
        Assert.AreEqual(Utc("2026-10-05T19:00:00Z"), ScopeRules.WindowEnd(windows, utc));
        Assert.IsNull(ScopeRules.WindowEnd(windows, Utc("2026-10-05T19:00:00Z")));
    }

    [TestMethod]
    public void MonthlyScheduleSkipsMonthsWithoutDay31()
    {
        DateTime start = Utc("2026-01-31T01:00:00Z");
        Assert.AreEqual(Utc("2026-03-31T01:00:00Z"), Recurrence.Next("FREQ=MONTHLY;BYMONTHDAY=31", "Asia/Bangkok", start, start));
    }

    [TestMethod]
    public void SchedulePreservesLocalTimeAcrossDstAndSkipsNonexistentTime()
    {
        DateTime start = Utc("2026-03-07T07:30:00Z");
        Assert.AreEqual(Utc("2026-03-09T06:30:00Z"), Recurrence.Next("FREQ=DAILY", "America/New_York", start, start));
        Assert.IsNull(Recurrence.Next("FREQ=DAILY", "UTC", start, start, start.AddDays(1)));
        Assert.ThrowsExactly<BusinessException>(() => Recurrence.Next("FREQ=SECONDLY", "UTC", start, start));
    }

    [TestMethod]
    public void FindingIdentityIgnoresSecretQueryValuesButRetainsParameterNames()
    {
        string first = ScopeRules.Identity(1, "10020", "get", "https://example.invalid/a?token=secret&x=1", "x");
        string second = ScopeRules.Identity(1, "10020", "GET", "https://example.invalid/a?x=2&token=other", "x");
        Assert.AreEqual(first, second);
        Assert.AreNotEqual(first, ScopeRules.Identity(2, "10020", "GET", "https://example.invalid/a?x=2&token=other", "x"));
        Assert.IsFalse(first.Contains("secret", StringComparison.Ordinal));
    }

    [TestMethod]
    public void EmptyReportMustStillIdentifyAnInScopeSite()
    {
        byte[] valid = Encoding.UTF8.GetBytes("""{"@version":"2.17.0","site":[{"@name":"https://example.invalid/","alerts":[]}]}""");
        Assert.HasCount(0, ZapReports.Parse(valid, "JSON", Scope).Records);
        byte[] outside = Encoding.UTF8.GetBytes("""{"@version":"2.17.0","site":[{"@name":"https://evil.invalid/","alerts":[]}]}""");
        Assert.ThrowsExactly<BusinessException>(() => ZapReports.Parse(outside, "JSON", Scope));
        Assert.ThrowsExactly<BusinessException>(() => ZapReports.Parse(Encoding.UTF8.GetBytes("""{"@version":"2.17.0","site":[]}"""), "JSON", Scope));
    }

    [TestMethod]
    public void XmlParserRefusesExternalEntities()
    {
        byte[] xml = Encoding.UTF8.GetBytes("""<!DOCTYPE OWASPZAPReport [<!ENTITY x SYSTEM "file:///etc/passwd">]><OWASPZAPReport version="2.17.0"><site name="https://example.invalid/">&x;</site></OWASPZAPReport>""");
        Assert.ThrowsExactly<System.Xml.XmlException>(() => ZapReports.Parse(xml, "XML", Scope));
    }

    [TestMethod]
    public void ReportRejectsOneInvalidRecordBeforeAnyImport()
    {
        byte[] json = Encoding.UTF8.GetBytes("""{"@version":"2.17.0","site":[{"@name":"https://example.invalid/","alerts":[{"pluginid":"10020","name":"X","riskcode":"2","confidence":"2","instances":[{"uri":"https://example.invalid/"},{"uri":"https://evil.invalid/"}]}]}]}""");
        Assert.ThrowsExactly<BusinessException>(() => ZapReports.Parse(json, "JSON", Scope));
    }

    [TestMethod]
    public void EncryptionAuthenticatesAndFingerprintIsKeyed()
    {
        string root = Path.Combine(Path.GetTempPath(), "htqlhdk-core-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string key = Path.Combine(root, "key"); File.WriteAllBytes(key, RandomNumberGenerator.GetBytes(32));
        try
        {
            Protection protection = new(key, root);
            byte[] data = Encoding.UTF8.GetBytes("private raw report");
            byte[] cipher = protection.Encrypt(data);
            CollectionAssert.AreEqual(data, protection.Decrypt(cipher));
            Assert.AreEqual(protection.Fingerprint("a"), protection.Fingerprint("a"));
            Assert.AreNotEqual(protection.Fingerprint("a"), protection.Fingerprint("b"));
            cipher[^1] ^= 1;
            Assert.ThrowsExactly<AuthenticationTagMismatchException>(() => protection.Decrypt(cipher));
        }
        finally { File.Delete(key); Directory.Delete(root); }
    }

    [TestMethod]
    public void MaskAndCsvNeutralizeSecretsAndFormulaInjection()
    {
        string masked = Protection.Mask("url https://example.invalid/?token=topsecret password=hello");
        Assert.IsFalse(masked.Contains("topsecret", StringComparison.Ordinal));
        Assert.IsFalse(masked.Contains("hello", StringComparison.Ordinal));
        Assert.AreEqual("\"'=HYPERLINK(\"\"evil\"\")\"", CoreService.CsvCell("=HYPERLINK(\"evil\")"));
        Assert.AreEqual("\"normal\"", CoreService.CsvCell("normal"));
    }

    private static DateTime Utc(string value) => DateTimeOffset.Parse(value, CultureInfo.InvariantCulture).UtcDateTime;
}
