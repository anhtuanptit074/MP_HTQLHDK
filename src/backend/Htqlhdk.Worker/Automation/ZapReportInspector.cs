using System.Security.Cryptography;
using System.Text.Json;

namespace Htqlhdk.Worker.Automation;

public sealed class ZapReportInspector
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "This stateless inspector is registered through dependency injection for replacement in tests and deployments.")]
    public async Task<ZapReportMetadata> InspectAsync(
        string reportPath,
        IEnumerable<string> expectedTargets,
        long maxBytes,
        CancellationToken cancellationToken)
    {
        FileInfo file = new(reportPath);
        if (!file.Exists || file.Length == 0 || file.Length > maxBytes)
        {
            throw new InvalidDataException("ZAP JSON report is missing, empty, or over the configured limit.");
        }

        await using FileStream stream = new(
            file.FullName,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            64 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        using JsonDocument document = await JsonDocument.ParseAsync(
            stream,
            new JsonDocumentOptions { MaxDepth = 128 },
            cancellationToken);

        JsonElement root = document.RootElement;
        string programName = root.TryGetProperty("@programName", out JsonElement program)
            ? program.GetString() ?? string.Empty
            : string.Empty;
        if (!programName.Contains("ZAP", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("Report does not identify OWASP ZAP as the producer.");
        }

        string version = root.TryGetProperty("@version", out JsonElement versionNode)
            ? versionNode.GetString() ?? string.Empty
            : string.Empty;
        if (string.IsNullOrWhiteSpace(version))
        {
            throw new InvalidDataException("ZAP report version is missing.");
        }

        if (!root.TryGetProperty("site", out JsonElement sites) || sites.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidDataException("Traditional JSON report site array is missing.");
        }

        HashSet<string> expectedHosts = expectedTargets
            .Select(value => new Uri(value, UriKind.Absolute).IdnHost)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        HashSet<string> reportedHosts = new(StringComparer.OrdinalIgnoreCase);
        int siteCount = 0;
        int alertCount = 0;
        foreach (JsonElement site in sites.EnumerateArray())
        {
            siteCount++;
            if (!site.TryGetProperty("@name", out JsonElement nameNode) ||
                !Uri.TryCreate(nameNode.GetString(), UriKind.Absolute, out Uri? siteUri) ||
                !expectedHosts.Contains(siteUri.IdnHost))
            {
                throw new InvalidDataException("Report contains an invalid or unexpected site.");
            }

            reportedHosts.Add(siteUri.IdnHost);

            if (site.TryGetProperty("alerts", out JsonElement alerts) && alerts.ValueKind == JsonValueKind.Array)
            {
                alertCount += alerts.GetArrayLength();
            }
        }

        if (siteCount == 0 || !reportedHosts.SetEquals(expectedHosts))
        {
            throw new InvalidDataException("Report does not cover every scheduled target host.");
        }

        stream.Position = 0;
        string sha256 = Convert.ToHexString(await SHA256.HashDataAsync(stream, cancellationToken))
            .ToLowerInvariant();
        return new ZapReportMetadata(version, file.Length, sha256, siteCount, alertCount);
    }
}
