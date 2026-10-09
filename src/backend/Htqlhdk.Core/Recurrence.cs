using System.Globalization;

namespace Htqlhdk.Core;

public static class Recurrence
{
    private static readonly string[] DayCodes = ["SU", "MO", "TU", "WE", "TH", "FR", "SA"];
    public static DateTime? Next(string rule, string timezone, DateTime effectiveFrom, DateTime after, DateTime? effectiveUntil = null)
    {
        Dictionary<string, string> fields = [];
        foreach (string part in rule.Split(';'))
        {
            string[] pair = part.Split('=');
            Fields.Require(pair.Length == 2 && fields.TryAdd(pair[0], pair[1]), "RRULE không hợp lệ.");
        }
        Fields.Require(fields.Keys.All(k => k is "FREQ" or "INTERVAL" or "BYDAY" or "BYMONTHDAY") && fields.TryGetValue("FREQ", out _), "RRULE có thành phần chưa hỗ trợ.");
        string frequency = fields["FREQ"];
        Fields.Require(frequency is "DAILY" or "WEEKLY" or "MONTHLY", "Chỉ hỗ trợ DAILY, WEEKLY, MONTHLY.");
        int interval = fields.TryGetValue("INTERVAL", out string? intervalText) ? int.Parse(intervalText, CultureInfo.InvariantCulture) : 1;
        Fields.Require(interval is >= 1 and <= 365, "INTERVAL không hợp lệ.");
        Fields.Require(!fields.ContainsKey("BYDAY") || frequency == "WEEKLY", "BYDAY chỉ dùng cho WEEKLY.");
        Fields.Require(!fields.ContainsKey("BYMONTHDAY") || frequency == "MONTHLY", "BYMONTHDAY chỉ dùng cho MONTHLY.");
        int weekday = fields.TryGetValue("BYDAY", out string? day) ? Array.IndexOf(DayCodes, day) : -1;
        Fields.Require(!fields.ContainsKey("BYDAY") || weekday >= 0, "BYDAY chỉ nhận một thứ hợp lệ.");
        int monthDay = fields.TryGetValue("BYMONTHDAY", out string? dateText) ? int.Parse(dateText, CultureInfo.InvariantCulture) : 0;
        Fields.Require(!fields.ContainsKey("BYMONTHDAY") || monthDay is >= 1 and <= 31, "BYMONTHDAY không hợp lệ.");
        TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
        DateTime start = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(effectiveFrom, DateTimeKind.Utc), zone);
        DateTime localAfter = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(after, DateTimeKind.Utc), zone);
        DateTime search = localAfter.Date > start.Date ? localAfter.Date : start.Date;
        for (int offset = 0; offset < 36600; offset++)
        {
            DateTime date = search.AddDays(offset);
            int days = (date - start.Date).Days;
            int months = (date.Year - start.Year) * 12 + date.Month - start.Month;
            int startWeek = start.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)start.DayOfWeek;
            bool match = frequency switch
            {
                "DAILY" => days % interval == 0,
                "WEEKLY" => ((days + startWeek - 1) / 7) % interval == 0 && (int)date.DayOfWeek == (weekday >= 0 ? weekday : (int)start.DayOfWeek),
                _ => months % interval == 0 && date.Day == (monthDay > 0 ? monthDay : start.Day)
            };
            if (!match) { continue; }
            DateTime local = DateTime.SpecifyKind(date + start.TimeOfDay, DateTimeKind.Unspecified);
            if (zone.IsInvalidTime(local)) { continue; }
            DateTime utc = TimeZoneInfo.ConvertTimeToUtc(local, zone);
            if (effectiveUntil is not null && utc >= effectiveUntil) { return null; }
            if (utc >= effectiveFrom && utc > after) { return utc; }
        }
        return null;
    }
}
