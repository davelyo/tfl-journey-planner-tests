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

    [When("I edit preferences to show {string}")]
    public async Task WhenIEditPreferencesToShow(string preference)
    {
        await resultsPage.EditPreferencesAsync();
        await resultsPage.ChoosePreferenceAsync(preference);
    }

    [When("I update my journey")]
    public async Task WhenIUpdateMyJourney()
    {
        await resultsPage.UpdateJourneyAsync();
    }

    [Given("I have edited preferences to show {string}")]
    public async Task GivenIHaveEditedPreferencesToShow(string preference)
    {
        await WhenIEditPreferencesToShow(preference);
        await WhenIUpdateMyJourney();
    }

    [Then("the results show routes with least walking")]
    public async Task ThenTheResultsShowRoutesWithLeastWalking()
    {
        await Assertions.Expect(resultsPage
            .PreferenceSummary("Showing routes with least walking")).ToBeVisibleAsync();
    }

    [Then("the first journey option shows a journey time in minutes")]
    public async Task ThenTheFirstJourneyOptionShowsAJourneyTimeInMinutes()
    {
        var minutes = await resultsPage.GetFirstOptionMinutesAsync();
        Assert.That(minutes, Is.GreaterThan(0), "first journey option time in minutes");
    }

    [When("I view the details of the first journey option")]
    public async Task WhenIViewTheDetailsOfTheFirstJourneyOption()
    {
        await resultsPage.ViewDetailsOfFirstOptionAsync();
    }

    [Then("the access information at {string} is:")]
    public async Task ThenTheAccessInformationAtIs(string station, Table table)
    {
        var expected = table.Rows.Select(row => row["Access information"]);
        var actual = await resultsPage.GetAccessInformationAsync(station);
        Assert.That(actual, Is.EquivalentTo(expected), $"access information at {station}");
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
