namespace TflJourneyPlanner.Tests.Support;

/// <summary>
/// Test settings for the TflJourneyPlanner.Tests project,
/// from the TestRunParameters section of test.runsettings.
/// </summary>
public class TestSettings
{
    public required string BaseUrl { get; init; }

    public required int NavigationTimeoutMs { get; init; }

    public static TestSettings Load() => new()
    {
        BaseUrl = GetRequired("BaseUrl"),
        NavigationTimeoutMs = int.Parse(GetRequired("NavigationTimeoutMs")),
    };

    private static string GetRequired(string parameterName) =>
        TestContext.Parameters.Get(parameterName)
            ?? throw new InvalidOperationException(
                $"{parameterName} parameter is missing from test.runsettings.");
}
