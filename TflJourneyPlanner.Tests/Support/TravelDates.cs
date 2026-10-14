namespace TflJourneyPlanner.Tests.Support;

/// <summary>
/// Picks a departure date so results are independent of when the test is run.
/// </summary>
public static class TravelDates
{
    private static DateOnly TodayInLondon() =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("Europe/London")).Date);

    public static DateOnly NextWeekdayInLondon()
    {
        var date = TodayInLondon().AddDays(1);
        while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            date = date.AddDays(1);
        }

        return date;
    }
}
