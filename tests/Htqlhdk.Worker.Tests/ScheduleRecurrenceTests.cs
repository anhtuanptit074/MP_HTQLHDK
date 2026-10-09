using Htqlhdk.Worker.Automation;

namespace Htqlhdk.Worker.Tests;

[TestClass]
public sealed class ScheduleRecurrenceTests
{
    [TestMethod]
    public void CalculateNextWeeklyRulePreservesLocalWallClock()
    {
        DateTimeOffset current = DateTimeOffset.Parse(
            "2026-09-07T15:00:00Z",
            System.Globalization.CultureInfo.InvariantCulture);

        DateTimeOffset next = ScheduleRecurrence.CalculateNext(
            current, "Weekly", "FREQ=WEEKLY;INTERVAL=2;BYDAY=MO", "Asia/Bangkok");

        Assert.AreEqual(
            DateTimeOffset.Parse("2026-09-21T15:00:00Z", System.Globalization.CultureInfo.InvariantCulture),
            next);
    }

    [TestMethod]
    public void CalculateNextCustomRuleIsRejected()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => ScheduleRecurrence.CalculateNext(
            DateTimeOffset.UtcNow, "Custom", "FREQ=HOURLY", "UTC"));
    }

    [TestMethod]
    public void CalculateNextUnknownOptionIsRejected()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => ScheduleRecurrence.CalculateNext(
            DateTimeOffset.UtcNow, "Daily", "FREQ=DAILY;COUNT=5", "UTC"));
    }

    [TestMethod]
    public void CalculateNextMonthlyDaySkipsMonthsWithoutThatDay()
    {
        DateTimeOffset current = DateTimeOffset.Parse(
            "2026-01-31T02:00:00Z",
            System.Globalization.CultureInfo.InvariantCulture);

        DateTimeOffset next = ScheduleRecurrence.CalculateNext(
            current, "Monthly", "FREQ=MONTHLY;BYMONTHDAY=31", "UTC");

        Assert.AreEqual(
            DateTimeOffset.Parse("2026-03-31T02:00:00Z", System.Globalization.CultureInfo.InvariantCulture),
            next);
    }
}
