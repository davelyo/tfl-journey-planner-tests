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

    [When("I enter {string} as the {LocationField} location")]
    public async Task EnterLocation(string text, LocationField field)
    {
        await plannerPage.EnterLocationAsync(field, text);
    }

    [Given("I plan my journey")]
    [When("I plan my journey")]
    [When("I plan my journey without entering any locations")]
    public async Task PlanMyJourney()
    {
        await plannerPage.PlanJourneyAsync();
    }

    [Then("the Journey Planner is shown")]
    public async Task ThenTheJourneyPlannerIsShown()
    {
        await Assertions.Expect(plannerPage.Heading).ToBeVisibleAsync();
    }

    [Then("the {LocationField} field warns {string}")]
    public async Task ThenTheFieldWarns(LocationField field, string message)
    {
        await Assertions.Expect(plannerPage.ValidationMessage(message)).ToBeVisibleAsync();

        // The message is also in the field's accessible description, so screen readers announce it.
        await Assertions.Expect(plannerPage.LocationInput(field))
            .ToHaveAccessibleDescriptionAsync(message);
    }

    [Then("I stay on the Journey Planner")]
    public async Task ThenIStayOnTheJourneyPlanner()
    {
        await Assertions.Expect(plannerPage.Heading).ToBeVisibleAsync();
        Assert.That(plannerPage.Url, Does.Not.Contain("/plan-a-journey/results"));
    }
}
