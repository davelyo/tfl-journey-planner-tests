using System.Text.RegularExpressions;

namespace TflJourneyPlanner.Tests.Support;

/// <summary>Parses journey time strings like "6mins" or "2hrs 23mins" into minutes.</summary>
public static class JourneyTimeParser
{
    public static int ToMinutes(string timeString)
    {
        var hours = Regex.Match(timeString, @"(\d+)\s*hrs?");
        var minutes = Regex.Match(timeString, @"(\d+)\s*mins?");

        if (!hours.Success && !minutes.Success)
        {
            throw new FormatException($"Invalid journey time: {timeString}");
        }

        return (hours.Success ? int.Parse(hours.Groups[1].Value) * 60 : 0)
            + (minutes.Success ? int.Parse(minutes.Groups[1].Value) : 0);
    }
}
