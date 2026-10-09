using Htqlhdk.Worker.Automation;

namespace Htqlhdk.Worker.Tests;

[TestClass]
public sealed class ZapPlanRendererTests
{
    [TestMethod]
    public void PrefixScopeCannotExpandToSimilarlyNamedSiblingPaths()
    {
        ZapJobSpecification job = CreateJob(false) with { TargetUrls = ["https://example.invalid/app"], ScopeRules = [new("Include", "URL", "https://example.invalid/app", "Prefix")] };
        string yaml = new ZapPlanRenderer().Render(job);
        StringAssert.Contains(yaml, "(?:/.*|\\\\?.*)?$");
        Assert.ThrowsExactly<ArgumentException>(() => new ZapPlanRenderer().Render(job with { TargetUrls = ["https://example.invalid/application"] }));
    }

    [TestMethod]
    public void XmlProfileProducesTraditionalXmlReport()
    {
        string yaml = new ZapPlanRenderer().Render(CreateJob(false), "XML");
        StringAssert.Contains(yaml, "traditional-xml"); StringAssert.Contains(yaml, "report.xml");
        Assert.IsFalse(yaml.Contains("traditional-json", StringComparison.Ordinal));
    }

    [TestMethod]
    public void RenderPassiveProfileOmitsActiveScanAndSecrets()
    {
        ZapJobSpecification job = CreateJob(active: false, authentication: ZapAuthenticationKind.Header);

        string yaml = new ZapPlanRenderer().Render(job);

        StringAssert.Contains(yaml, "type: spider");
        StringAssert.Contains(yaml, "template: \"traditional-json\"");
        Assert.IsFalse(yaml.Contains("type: activeScan", StringComparison.Ordinal));
        Assert.IsFalse(yaml.Contains("HTQLHDK_ZAP_AUTH_TOKEN", StringComparison.Ordinal));
        Assert.IsFalse(yaml.Contains("secret-value", StringComparison.Ordinal));
    }

    [TestMethod]
    public void RenderActiveProfileAddsBoundedActiveScan()
    {
        string yaml = new ZapPlanRenderer().Render(CreateJob(active: true));

        StringAssert.Contains(yaml, "type: activeScan");
        StringAssert.Contains(yaml, "maxScanDurationInMins: 30");
        StringAssert.Contains(yaml, "delayInMs: 100");
        StringAssert.Contains(yaml, "threadPerHost: 2");
    }

    [TestMethod]
    public void RenderScopeRulesProducesWholeUrlRegexes()
    {
        string yaml = new ZapPlanRenderer().Render(CreateJob(active: false));

        StringAssert.Contains(yaml, "^https?://example\\\\.invalid(?::[0-9]+)?(?:/.*)?$");
        StringAssert.Contains(yaml, "^https://example\\\\.invalid/logout$");
    }

    [TestMethod]
    public void RenderTargetOutsideApprovedScopeIsRejected()
    {
        ZapJobSpecification job = CreateJob(active: false) with
        {
            TargetUrls = ["https://other.invalid/"]
        };

        Assert.ThrowsExactly<ArgumentException>(() => new ZapPlanRenderer().Render(job));
    }

    private static ZapJobSpecification CreateJob(
        bool active,
        ZapAuthenticationKind authentication = ZapAuthenticationKind.None) =>
        new(
            1,
            "ZAE-TEST-0001",
            10,
            20,
            30,
            ["https://example.invalid/"],
            [
                new ZapScopeRule("Include", "Domain", "example.invalid", "Exact"),
                new ZapScopeRule("Exclude", "URL", "https://example.invalid/logout", "Exact")
            ],
            ZapSpiderKind.Traditional,
            active,
            authentication,
            authentication == ZapAuthenticationKind.Header ? "Authorization" : null,
            authentication == ZapAuthenticationKind.Header ? "env:HTQLHDK_ZAP_AUTH_TOKEN" : null,
            10,
            10,
            30,
            5,
            100,
            2,
            DateTimeOffset.Parse("2026-09-12T00:00:00Z", System.Globalization.CultureInfo.InvariantCulture),
            60);
}
