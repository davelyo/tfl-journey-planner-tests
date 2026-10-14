using Microsoft.Playwright;

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

    [Then("the results warn {string}")]
    public async Task ThenTheResultsWarn(string message)
    {
        await Assertions.Expect(resultsPage.ErrorMessage(message)).ToBeVisibleAsync();
    }

    [Then("no journey options are shown")]
    public async Task ThenNoJourneyOptionsAreShown()
    {
        // Results can appear as option rows or as Walking and Cycling boxes,
        // so check that none of them exists.
        await Assertions.Expect(resultsPage.JourneyOptions).ToHaveCountAsync(0);
        await Assertions.Expect(resultsPage.ModeOption("Walking")).ToHaveCountAsync(0);
        await Assertions.Expect(resultsPage.ModeOption("Cycling")).ToHaveCountAsync(0);
    }
}
