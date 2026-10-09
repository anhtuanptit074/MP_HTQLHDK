using Htqlhdk.Worker.Automation;

namespace Htqlhdk.Worker.Tests;

[TestClass]
public sealed class ZapReportInspectorTests
{
    [TestMethod]
    public async Task InspectAsyncTraditionalJsonReturnsMetadata()
    {
        string path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, """
                {
                  "@programName": "OWASP ZAP",
                  "@version": "synthetic-test-only",
                  "site": [{"@name":"https://example.invalid","alerts":[{"pluginid":"1"}]}]
                }
                """);

            ZapReportMetadata result = await new ZapReportInspector().InspectAsync(
                path, ["https://example.invalid/"], 1024 * 1024, CancellationToken.None);

            Assert.AreEqual("synthetic-test-only", result.ZapVersion);
            Assert.AreEqual(1, result.SiteCount);
            Assert.AreEqual(1, result.AlertCount);
            Assert.AreEqual(64, result.Sha256.Length);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public async Task InspectAsyncUnexpectedHostIsRejected()
    {
        string path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, """
                {"@programName":"OWASP ZAP","@version":"test","site":[{"@name":"https://other.invalid","alerts":[]}]}
                """);

            await Assert.ThrowsExactlyAsync<InvalidDataException>(() =>
                new ZapReportInspector().InspectAsync(
                    path, ["https://example.invalid/"], 1024 * 1024, CancellationToken.None));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public async Task InspectAsyncEmptySiteListIsRejected()
    {
        string path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, """
                {"@programName":"OWASP ZAP","@version":"test","site":[]}
                """);

            await Assert.ThrowsExactlyAsync<InvalidDataException>(() =>
                new ZapReportInspector().InspectAsync(
                    path, ["https://example.invalid/"], 1024 * 1024, CancellationToken.None));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
