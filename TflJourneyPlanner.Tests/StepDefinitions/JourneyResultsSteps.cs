using Reqnroll;

using TflJourneyPlanner.Tests.Pages;

namespace TflJourneyPlanner.Tests.StepDefinitions;

/// <summary>Steps that check the journey results.</summary>
[Binding]
public class JourneyResultsSteps(JourneyResultsPage resultsPage)
{
    [Then("the {word} time is shown in minutes")]
    public async Task ThenTheModeTimeIsShownInMinutes(string mode)
    {
        // Capitalise the mode ("Walking", "Cycling") to match the heading text.
        var heading = char.ToUpperInvariant(mode[0]) + mode[1..];
        var minutes = await resultsPage.GetModeMinutesAsync(heading);
        Assert.That(minutes, Is.GreaterThan(0), $"{heading} time in minutes");
    }
}
