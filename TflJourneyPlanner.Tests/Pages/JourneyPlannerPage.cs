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

    // Location inputs are comboboxes with visually hidden labels, e.g. "From location".
    private ILocator LocationInput(LocationField field) =>
        page.GetByRole(AriaRole.Combobox, new() { Name = $"{field} location", Exact = true });

    private ILocator Suggestion(string name) =>
        page.GetByRole(AriaRole.Option, new() { Name = name, Exact = true });

    private ILocator PlanMyJourneyButton =>
        page.GetByRole(AriaRole.Button, new() { Name = "Plan my journey" });

    public async Task OpenAsync()
    {
        // The site sometimes serves an empty page without the trailing slash (/plan-a-journey)
        await page.GotoAsync($"{settings.BaseUrl}/plan-a-journey/");
    }

    /// <summary>
    /// Types part of a place name so the autocomplete list opens,
    /// then chooses the matching suggestion.
    /// </summary>
    public async Task ChooseSuggestionAsync(
        LocationField field, string partialName, string suggestion)
    {
        await LocationInput(field).PressSequentiallyAsync(partialName);
        await Suggestion(suggestion).ClickAsync();
    }

    public async Task PlanJourneyAsync()
    {
        await PlanMyJourneyButton.ClickAsync();
    }
}
