namespace TflJourneyPlanner.Tests.Support;

/// <summary>
/// Test settings for the TflJourneyPlanner.Tests project,
/// from the TestRunParameters section of test.runsettings.
/// </summary>
public class TestSettings
{
    public required string BaseUrl { get; init; }

    public static TestSettings Load() => new()
    {
        BaseUrl = TestContext.Parameters.Get("BaseUrl")
            ?? throw new InvalidOperationException(
                "BaseUrl parameter is missing from test.runsettings."),
    };
}
