using System.Globalization;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace Htqlhdk.Core;

public sealed record ZapRecord(string Locator, string AlertId, string Title, string Url, string Method, string Parameter,
    string Severity, int Confidence, string Description, string Solution, int? Cwe, int? Wasc);
public sealed record ZapReport(string Version, string Format, IReadOnlyList<ZapRecord> Records);

public static class ZapReports
{
    private static readonly string[] Severities = ["Informational", "Low", "Medium", "High"];
    public static ZapReport Parse(byte[] bytes, string format, IReadOnlyList<ScopeRule> scope)
    {
        Fields.Require(bytes.Length is > 0 and <= 52428800, "Báo cáo vượt giới hạn 50 MB.", 413, "UPLOAD_SIZE");
        Fields.Require(format is "XML" or "JSON", "Định dạng báo cáo không hợp lệ.");
        ZapReport report = format == "XML" ? ParseXml(bytes, scope) : ParseJson(bytes, scope);
        Fields.Require(!string.IsNullOrWhiteSpace(report.Version), "Không nhận diện được phiên bản báo cáo OWASP ZAP.");
        Fields.Require(report.Records.Count <= 100000, "Báo cáo chứa quá nhiều bản ghi.");
        foreach (ZapRecord record in report.Records)
        {
            Fields.Require(ScopeRules.Contains(scope, record.Url), "Báo cáo có URL nằm ngoài phạm vi đã duyệt.", 422, "REPORT_SCOPE");
            Fields.Require(!string.IsNullOrWhiteSpace(record.AlertId) && !string.IsNullOrWhiteSpace(record.Title) && record.Confidence is >= 0 and <= 4,
                "Bản ghi ZAP không hợp lệ.", 422, "REPORT_SCHEMA");
        }
        return report;
    }
    private static ZapReport ParseJson(byte[] bytes, IReadOnlyList<ScopeRule> scope)
    {
        using JsonDocument document = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 64 });
        JsonElement root = document.RootElement;
        Fields.Require(root.ValueKind == JsonValueKind.Object && root.TryGetProperty("site", out _), "Yêu cầu Traditional JSON Report của OWASP ZAP.");
        List<ZapRecord> records = [];
        JsonElement[] sites = Many(root.GetProperty("site"));
        Fields.Require(sites.Length > 0, "Báo cáo phải chứa ít nhất một site đã được quét.", 422, "REPORT_SCHEMA");
        foreach (JsonElement site in sites)
        {
            Fields.Require(SiteMatches(scope, String(site, "@name", String(site, "name"))), "Site trong báo cáo nằm ngoài tài sản được duyệt.", 422, "REPORT_SCOPE");
            Fields.Require(site.TryGetProperty("alerts", out JsonElement alerts), "Site thiếu danh sách alerts.", 422, "REPORT_SCHEMA");
            foreach (JsonElement alert in Many(alerts))
            {
                int risk = Integer(alert, "riskcode") ?? -1;
                Fields.Require(risk is >= 0 and <= 3, "Source Risk không hợp lệ.");
                Fields.Require(alert.TryGetProperty("instances", out _), "Cảnh báo thiếu instances.");
                foreach (JsonElement instance in Many(alert.GetProperty("instances")))
                {
                    records.Add(new(records.Count.ToString(CultureInfo.InvariantCulture), String(alert, "pluginid"), String(alert, "name", String(alert, "alert")),
                        String(instance, "uri"), String(instance, "method", "GET").ToUpperInvariant(), String(instance, "param"), Severities[risk],
                        Integer(alert, "confidence") ?? -1, String(alert, "desc"), String(alert, "solution"), Integer(alert, "cweid"), Integer(alert, "wascid")));
                }
            }
        }
        return new(String(root, "@version", String(root, "version")), "JSON", records);
    }
    private static ZapReport ParseXml(byte[] bytes, IReadOnlyList<ScopeRule> scope)
    {
        using MemoryStream stream = new(bytes);
        using XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 52428800 });
        XDocument document = XDocument.Load(reader);
        XElement root = document.Root ?? throw new BusinessException(422, "REPORT_SCHEMA", "Báo cáo XML rỗng.");
        Fields.Require(root.Name.LocalName is "OWASPZAPReport" or "ZAPReport", "Yêu cầu Traditional XML Report của OWASP ZAP.");
        XElement[] sites = root.Elements("site").ToArray();
        Fields.Require(sites.Length > 0, "Báo cáo phải chứa ít nhất một site đã được quét.", 422, "REPORT_SCHEMA");
        foreach (XElement site in sites) { Fields.Require(SiteMatches(scope, site.Attribute("name")?.Value ?? ""), "Site trong báo cáo nằm ngoài tài sản được duyệt.", 422, "REPORT_SCOPE"); }
        List<ZapRecord> records = [];
        foreach (XElement alert in root.Descendants("alertitem"))
        {
            int risk = ParseInt(alert.Element("riskcode")?.Value) ?? -1;
            Fields.Require(risk is >= 0 and <= 3, "Source Risk không hợp lệ.");
            XElement[] instances = alert.Descendants("instance").ToArray();
            Fields.Require(instances.Length > 0, "Cảnh báo thiếu instances.");
            foreach (XElement instance in instances)
            {
                records.Add(new(records.Count.ToString(CultureInfo.InvariantCulture), alert.Element("pluginid")?.Value ?? "", alert.Element("alert")?.Value ?? "",
                    instance.Element("uri")?.Value ?? "", (instance.Element("method")?.Value ?? "GET").ToUpperInvariant(), instance.Element("param")?.Value ?? "",
                    Severities[risk], ParseInt(alert.Element("confidence")?.Value) ?? -1, alert.Element("desc")?.Value ?? "", alert.Element("solution")?.Value ?? "",
                    ParseInt(alert.Element("cweid")?.Value), ParseInt(alert.Element("wascid")?.Value)));
            }
        }
        return new(root.Attribute("version")?.Value ?? "", "XML", records);
    }
    private static JsonElement[] Many(JsonElement value) => value.ValueKind == JsonValueKind.Array ? value.EnumerateArray().ToArray() : [value];
    private static bool SiteMatches(IReadOnlyList<ScopeRule> scope, string value)
    {
        Uri site = ScopeRules.SafeUrl(value);
        return scope.Any(rule => rule.Effect == "Include" && (rule.TargetType == "URL"
            ? ScopeRules.SafeUrl(rule.TargetValue).GetLeftPart(UriPartial.Authority) == site.GetLeftPart(UriPartial.Authority)
            : ScopeRules.Contains([rule], value)));
    }
    private static string String(JsonElement value, string key, string fallback = "") => value.TryGetProperty(key, out JsonElement found) ? found.ToString() : fallback;
    private static int? Integer(JsonElement value, string key) => ParseInt(String(value, key));
    private static int? ParseInt(string? value) => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result) ? result : null;
}
