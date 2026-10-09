using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Htqlhdk.Worker.Automation;

public sealed class ZapPlanRenderer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "This stateless renderer is registered through dependency injection for replacement in tests and deployments.")]
    public string Render(ZapJobSpecification job, string reportFormat = "JSON")
    {
        ArgumentNullException.ThrowIfNull(job);
        Validate(job);

        string[] includes = BuildRegexes(job, "Include");
        string[] excludes = BuildRegexes(job, "Exclude");
        StringBuilder yaml = new();

        yaml.AppendLine("---");
        yaml.AppendLine("env:");
        yaml.AppendLine("  contexts:");
        yaml.AppendLine("    - name: \"managed-target\"");
        yaml.AppendLine("      urls:");
        foreach (string target in job.TargetUrls)
        {
            yaml.Append("        - ").AppendLine(Quote(target));
        }

        yaml.AppendLine("      includePaths:");
        foreach (string include in includes)
        {
            yaml.Append("        - ").AppendLine(Quote(include));
        }

        yaml.AppendLine("      excludePaths:");
        foreach (string exclude in excludes)
        {
            yaml.Append("        - ").AppendLine(Quote(exclude));
        }

        yaml.AppendLine("  parameters:");
        yaml.AppendLine("    failOnError: true");
        yaml.AppendLine("    failOnWarning: false");
        yaml.AppendLine("    continueOnFailure: false");
        yaml.AppendLine("    progressToStdout: true");
        yaml.AppendLine("jobs:");

        foreach (string target in job.TargetUrls)
        {
            if (job.SpiderKind == ZapSpiderKind.Traditional)
            {
                yaml.AppendLine("  - type: spider");
                yaml.AppendLine("    parameters:");
                yaml.AppendLine("      context: \"managed-target\"");
                yaml.Append("      url: ").AppendLine(Quote(target));
                yaml.Append("      maxDuration: ").AppendLine(job.SpiderMaxMinutes.ToString(System.Globalization.CultureInfo.InvariantCulture));
                yaml.AppendLine("      threadCount: 2");
                yaml.AppendLine("      parseGit: false");
                yaml.AppendLine("      parseDsStore: false");
                yaml.AppendLine("      parseSVNEntries: false");
            }
            else
            {
                yaml.AppendLine("  - type: spiderClient");
                yaml.AppendLine("    parameters:");
                yaml.AppendLine("      context: \"managed-target\"");
                yaml.Append("      url: ").AppendLine(Quote(target));
                yaml.Append("      maxDuration: ").AppendLine(job.SpiderMaxMinutes.ToString(System.Globalization.CultureInfo.InvariantCulture));
                yaml.AppendLine("      numberOfBrowsers: 1");
                yaml.AppendLine("      scopeCheck: \"Strict\"");
            }
        }

        AppendPassiveWait(yaml, job.PassiveWaitMaxMinutes);

        if (job.ActiveScanEnabled)
        {
            yaml.AppendLine("  - type: activeScan");
            yaml.AppendLine("    parameters:");
            yaml.AppendLine("      context: \"managed-target\"");
            yaml.AppendLine("      defaultStrength: \"Medium\"");
            yaml.AppendLine("      defaultThreshold: \"Medium\"");
            yaml.Append("      maxRuleDurationInMins: ").AppendLine(job.MaxRuleMinutes.ToString(System.Globalization.CultureInfo.InvariantCulture));
            yaml.Append("      maxScanDurationInMins: ").AppendLine(job.ActiveScanMaxMinutes.ToString(System.Globalization.CultureInfo.InvariantCulture));
            yaml.Append("      delayInMs: ").AppendLine(job.RequestDelayMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
            yaml.Append("      threadPerHost: ").AppendLine(job.ThreadPerHost.ToString(System.Globalization.CultureInfo.InvariantCulture));
            yaml.AppendLine("      handleAntiCSRFTokens: true");
            AppendPassiveWait(yaml, job.PassiveWaitMaxMinutes);
        }

        if (reportFormat == "XML") { AppendReport(yaml, "traditional-xml", "report.xml", "Traditional XML"); }
        else { AppendReport(yaml, "traditional-json", "report.json", "Traditional JSON"); }
        AppendReport(yaml, "traditional-html", "report.html", "Traditional HTML");

        return yaml.ToString();
    }

    private static void Validate(ZapJobSpecification job)
    {
        if (!Regex.IsMatch(job.ExecutionCode, "^[A-Za-z0-9_-]{1,40}$", RegexOptions.CultureInvariant))
        {
            throw new ArgumentException("Execution code contains unsafe characters.", nameof(job));
        }

        if (job.TargetUrls.Length == 0)
        {
            throw new ArgumentException("At least one target URL is required.", nameof(job));
        }

        foreach (string value in job.TargetUrls)
        {
            if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
                !string.IsNullOrEmpty(uri.UserInfo))
            {
                throw new ArgumentException("Every target must be an absolute HTTP(S) URL without user info.", nameof(job));
            }
        }

        string[] includePatterns = job.ScopeRules
            .Where(rule => string.Equals(rule.Effect, "Include", StringComparison.Ordinal))
            .Select(ToFullUrlRegex)
            .ToArray();
        string[] excludePatterns = job.ScopeRules
            .Where(rule => string.Equals(rule.Effect, "Exclude", StringComparison.Ordinal))
            .Select(ToFullUrlRegex)
            .ToArray();
        if (includePatterns.Length == 0)
        {
            throw new ArgumentException("The approved scope must contain an include rule.", nameof(job));
        }

        TimeSpan matchTimeout = TimeSpan.FromSeconds(1);
        foreach (string target in job.TargetUrls)
        {
            bool included = includePatterns.Any(pattern => Regex.IsMatch(
                target, pattern, RegexOptions.CultureInvariant, matchTimeout));
            bool excluded = excludePatterns.Any(pattern => Regex.IsMatch(
                target, pattern, RegexOptions.CultureInvariant, matchTimeout));
            if (!included || excluded)
            {
                throw new ArgumentException("A scheduled target is outside the approved scope.", nameof(job));
            }
        }

        if (job.AuthenticationKind == ZapAuthenticationKind.Header &&
            (string.IsNullOrWhiteSpace(job.AuthenticationHeaderName) ||
             string.IsNullOrWhiteSpace(job.AuthenticationSecretReference)))
        {
            throw new ArgumentException("Header authentication requires a header name and secret reference.", nameof(job));
        }

        if (job.AuthenticationKind == ZapAuthenticationKind.Header)
        {
            string authority = new Uri(job.TargetUrls[0], UriKind.Absolute).GetLeftPart(UriPartial.Authority);
            if (job.TargetUrls.Any(target => !string.Equals(
                new Uri(target, UriKind.Absolute).GetLeftPart(UriPartial.Authority),
                authority,
                StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException(
                    "Header authentication requires all targets to use one authority.",
                    nameof(job));
            }
        }
    }

    private static string[] BuildRegexes(ZapJobSpecification job, string effect)
    {
        string[] values = job.ScopeRules
            .Where(rule => string.Equals(rule.Effect, effect, StringComparison.Ordinal))
            .Select(ToFullUrlRegex)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (values.Length > 0 || effect == "Exclude")
        {
            return values;
        }

        return job.TargetUrls.Select(target => $"^{Regex.Escape(target.TrimEnd('/'))}(?:/.*)?$").ToArray();
    }

    private static string ToFullUrlRegex(ZapScopeRule rule)
    {
        return (rule.TargetType, rule.MatchMode) switch
        {
            ("Domain", "Exact") => $"^https?://{Regex.Escape(rule.TargetValue)}(?::[0-9]+)?(?:/.*)?$",
            ("Subdomain", "Exact") => $"^https?://(?:[^./]+\\.)+{Regex.Escape(rule.TargetValue)}(?::[0-9]+)?(?:/.*)?$",
            ("URL", "Exact") => $"^{Regex.Escape(rule.TargetValue)}$",
            ("URL", "Prefix") => $"^{Regex.Escape(rule.TargetValue.TrimEnd('/'))}(?:/.*|\\?.*)?$",
            _ => throw new InvalidOperationException("Unsupported approved scope rule.")
        };
    }

    private static void AppendPassiveWait(StringBuilder yaml, int maxMinutes)
    {
        yaml.AppendLine("  - type: passiveScan-wait");
        yaml.AppendLine("    parameters:");
        yaml.Append("      maxDuration: ").AppendLine(maxMinutes.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    private static void AppendReport(StringBuilder yaml, string template, string file, string title)
    {
        yaml.AppendLine("  - type: report");
        yaml.AppendLine("    parameters:");
        yaml.Append("      template: ").AppendLine(Quote(template));
        yaml.AppendLine("      reportDir: \"/zap/wrk/output\"");
        yaml.Append("      reportFile: ").AppendLine(Quote(file));
        yaml.Append("      reportTitle: ").AppendLine(Quote($"HTQLHDK {title} Report"));
        yaml.AppendLine("      displayReport: false");
    }

    private static string Quote(string value) => JsonSerializer.Serialize(value, JsonOptions);
}
