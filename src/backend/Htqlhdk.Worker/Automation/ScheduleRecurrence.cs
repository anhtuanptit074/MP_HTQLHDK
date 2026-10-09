using System.Diagnostics;

namespace Htqlhdk.Worker.Automation;

public static class ScheduleRecurrence
{
    private static readonly Dictionary<string, DayOfWeek> DayCodes =
        new Dictionary<string, DayOfWeek>(StringComparer.Ordinal)
        {
            ["MO"] = DayOfWeek.Monday,
            ["TU"] = DayOfWeek.Tuesday,
            ["WE"] = DayOfWeek.Wednesday,
            ["TH"] = DayOfWeek.Thursday,
            ["FR"] = DayOfWeek.Friday,
            ["SA"] = DayOfWeek.Saturday,
            ["SU"] = DayOfWeek.Sunday
        };

    public static DateTimeOffset CalculateNext(
        DateTimeOffset currentDue,
        string frequency,
        string recurrenceRule,
        string timezone)
    {
        Dictionary<string, string> rule = ParseRule(recurrenceRule);

        string expectedFrequency = frequency.ToUpperInvariant();
        if (!rule.TryGetValue("FREQ", out string? actualFrequency) || actualFrequency != expectedFrequency)
        {
            throw new InvalidOperationException("Recurrence FREQ must match schedule frequency.");
        }

        int interval = 1;
        if (rule.TryGetValue("INTERVAL", out string? intervalValue) &&
            (!int.TryParse(intervalValue, out interval) || interval is < 1 or > 365))
        {
            throw new InvalidOperationException("Recurrence INTERVAL is invalid.");
        }

        HashSet<string> allowedKeys = expectedFrequency switch
        {
            "DAILY" => ["FREQ", "INTERVAL"],
            "WEEKLY" => ["FREQ", "INTERVAL", "BYDAY"],
            "MONTHLY" => ["FREQ", "INTERVAL", "BYMONTHDAY"],
            _ => throw new InvalidOperationException("Only Daily, Weekly and Monthly automated recurrences are supported.")
        };
        if (rule.Keys.Any(key => !allowedKeys.Contains(key)))
        {
            throw new InvalidOperationException("Recurrence contains an unsupported option.");
        }

        TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
        DateTime local = TimeZoneInfo.ConvertTime(currentDue, zone).DateTime;
        DateTime nextLocal = expectedFrequency switch
        {
            "DAILY" => local.AddDays(interval),
            "WEEKLY" => CalculateWeekly(local, interval, rule),
            "MONTHLY" => CalculateMonthly(local, interval, rule),
            _ => throw new UnreachableException()
        };
        DateTime utc = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(nextLocal, DateTimeKind.Unspecified),
            zone);
        return new DateTimeOffset(utc);
    }

    private static Dictionary<string, string> ParseRule(string recurrenceRule)
    {
        Dictionary<string, string> result = new(StringComparer.Ordinal);
        foreach (string part in recurrenceRule.Split(
                     ';',
                     StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string[] pair = part.Split('=', 2, StringSplitOptions.TrimEntries);
            if (pair.Length != 2 || string.IsNullOrWhiteSpace(pair[0]) || string.IsNullOrWhiteSpace(pair[1]) ||
                !result.TryAdd(pair[0].ToUpperInvariant(), pair[1].ToUpperInvariant()))
            {
                throw new InvalidOperationException("Recurrence rule is malformed or contains duplicate options.");
            }
        }

        return result;
    }

    private static DateTime CalculateWeekly(
        DateTime local,
        int interval,
        Dictionary<string, string> rule)
    {
        if (!rule.TryGetValue("BYDAY", out string? dayCode))
        {
            return local.AddDays(7 * interval);
        }

        if (!DayCodes.TryGetValue(dayCode, out DayOfWeek targetDay))
        {
            throw new InvalidOperationException("Weekly BYDAY must contain one weekday code.");
        }

        int daysAhead = ((int)targetDay - (int)local.DayOfWeek + 7) % 7;
        return local.AddDays(daysAhead == 0 ? 7 * interval : daysAhead);
    }

    private static DateTime CalculateMonthly(
        DateTime local,
        int interval,
        Dictionary<string, string> rule)
    {
        if (!rule.TryGetValue("BYMONTHDAY", out string? dayValue))
        {
            return local.AddMonths(interval);
        }

        if (!int.TryParse(dayValue, out int targetDay) || targetDay is < 1 or > 31)
        {
            throw new InvalidOperationException("Monthly BYMONTHDAY must be between 1 and 31.");
        }

        DateTime candidate = new DateTime(
                local.Year,
                local.Month,
                1,
                local.Hour,
                local.Minute,
                local.Second,
                local.Millisecond,
                DateTimeKind.Unspecified)
            .AddTicks(local.Ticks % TimeSpan.TicksPerMillisecond)
            .AddMonths(interval);
        while (DateTime.DaysInMonth(candidate.Year, candidate.Month) < targetDay)
        {
            candidate = candidate.AddMonths(interval);
        }

        return candidate.AddDays(targetDay - 1);
    }
}
