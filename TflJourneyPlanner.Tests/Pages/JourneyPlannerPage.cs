using Microsoft.Playwright;

using TflJourneyPlanner.Tests.Support;

namespace TflJourneyPlanner.Tests.Pages;

/// <summary>
/// The "Plan a journey" widget at /plan-a-journey. Owns its locators and interactions.
/// </summary>
public class JourneyPlannerPage(IPage page, TestSettings settings)
{
    public ILocator Heading =>
        page.GetByRole(AriaRole.Heading, new() { Name = "Plan a journey", Level = 1 });

    public async Task OpenAsync()
    {
        // The site sometimes serves an empty page without the trailing slash (/plan-a-journey)
        await page.GotoAsync($"{settings.BaseUrl}/plan-a-journey/");
    }
}
