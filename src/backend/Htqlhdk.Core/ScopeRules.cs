using System.Globalization;

namespace Htqlhdk.Core;

public sealed record ScopeRule(string Effect, string TargetType, string TargetValue, string MatchMode);
public sealed record ScanWindow(int Weekday, TimeSpan Start, TimeSpan End, bool NextDay, string Timezone);

public static class ScopeRules
{
    public static Uri SafeUrl(string value)
    {
        Fields.Require(Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) && uri.Scheme is "http" or "https" &&
            string.IsNullOrEmpty(uri.UserInfo) && string.IsNullOrEmpty(uri.Fragment), "URL phải là HTTP/HTTPS, không chứa thông tin đăng nhập hoặc fragment.");
        return uri!;
    }
    public static bool Contains(IEnumerable<ScopeRule> rules, string url)
    {
        Uri target = SafeUrl(url);
        ScopeRule[] list = rules.ToArray();
        return list.Any(r => r.Effect == "Include" && Matches(r, target)) && !list.Any(r => r.Effect == "Exclude" && Matches(r, target));
    }
    private static bool Matches(ScopeRule rule, Uri target)
    {
        if (rule.TargetType == "Domain") { return target.IdnHost.Equals(rule.TargetValue.TrimEnd('.'), StringComparison.OrdinalIgnoreCase); }
        if (rule.TargetType == "Subdomain") { return target.IdnHost.EndsWith("." + rule.TargetValue.TrimEnd('.'), StringComparison.OrdinalIgnoreCase); }
        Uri allowed = SafeUrl(rule.TargetValue);
        if (!target.GetLeftPart(UriPartial.Authority).Equals(allowed.GetLeftPart(UriPartial.Authority), StringComparison.OrdinalIgnoreCase)) { return false; }
        return rule.MatchMode == "Exact" ? target.PathAndQuery == allowed.PathAndQuery :
            target.AbsolutePath.TrimEnd('/') == allowed.AbsolutePath.TrimEnd('/') || target.AbsolutePath.StartsWith(allowed.AbsolutePath.TrimEnd('/') + "/", StringComparison.Ordinal);
    }
    public static DateTime? WindowEnd(IEnumerable<ScanWindow> windows, DateTime utc)
    {
        DateTime? latest = null;
        foreach (ScanWindow window in windows)
        {
            TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(window.Timezone);
            DateTime local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), zone);
            foreach (int offset in new[] { 0, -1 })
            {
                DateTime day = local.Date.AddDays(offset);
                int weekday = day.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)day.DayOfWeek;
                DateTime start = day + window.Start;
                DateTime end = day + window.End + (window.NextDay ? TimeSpan.FromDays(1) : TimeSpan.Zero);
                if (weekday == window.Weekday && local >= start && local < end)
                {
                    DateTime until = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(end, DateTimeKind.Unspecified), zone);
                    if (latest is null || until > latest) { latest = until; }
                }
            }
        }
        return latest;
    }
    public static string Identity(long assetId, string alert, string method, string url, string parameter)
    {
        Uri uri = SafeUrl(url);
        string queryKeys = string.Join('&', uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Split('=')[0]).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal));
        return string.Join('\n', assetId.ToString(CultureInfo.InvariantCulture), alert, method.ToUpperInvariant(),
            uri.GetLeftPart(UriPartial.Authority).ToLowerInvariant(), uri.AbsolutePath, queryKeys, parameter);
    }
}
