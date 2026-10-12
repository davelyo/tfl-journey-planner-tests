using Microsoft.Playwright;

using Reqnroll;

using TflJourneyPlanner.Tests.Pages;

namespace TflJourneyPlanner.Tests.StepDefinitions;

/// <summary>
/// Steps that drive the Journey Planner search form.
/// </summary>
[Binding]
public class JourneyPlannerSteps(JourneyPlannerPage plannerPage)
{
    [When("I open the Journey Planner")]
    public async Task WhenIOpenTheJourneyPlanner()
    {
        await plannerPage.OpenAsync();
    }

    [Then("the Journey Planner is shown")]
    public async Task ThenTheJourneyPlannerIsShown()
    {
        await Assertions.Expect(plannerPage.Heading).ToBeVisibleAsync();
    }
}
