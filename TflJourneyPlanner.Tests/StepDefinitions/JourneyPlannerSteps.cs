using Microsoft.Playwright;

using Reqnroll;

using TflJourneyPlanner.Tests.Pages;
using TflJourneyPlanner.Tests.Support;

namespace TflJourneyPlanner.Tests.StepDefinitions;

/// <summary>Steps that drive the Journey Planner search form.</summary>
[Binding]
public class JourneyPlannerSteps(JourneyPlannerPage plannerPage)
{
    [Given("the Journey Planner is open")]
    public async Task GivenTheJourneyPlannerIsOpen()
    {
        await plannerPage.OpenAsync();
    }

    [Given("I type {string} in {LocationField} and choose {string}")]
    [When("I type {string} in {LocationField} and choose {string}")]
    public async Task ChooseSuggestion(string partialName, LocationField field, string suggestion)
    {
        await plannerPage.ChooseSuggestionAsync(field, partialName, suggestion);
    }

    [Given("I plan my journey")]
    [When("I plan my journey")]
    public async Task PlanMyJourney()
    {
        await plannerPage.PlanJourneyAsync();
    }

    [Then("the Journey Planner is shown")]
    public async Task ThenTheJourneyPlannerIsShown()
    {
        await Assertions.Expect(plannerPage.Heading).ToBeVisibleAsync();
    }
}
