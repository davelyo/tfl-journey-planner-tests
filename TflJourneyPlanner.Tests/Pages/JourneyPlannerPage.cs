using System.Globalization;

using Microsoft.Playwright;

using TflJourneyPlanner.Tests.Support;

namespace TflJourneyPlanner.Tests.Pages;

/// <summary>
/// The "Plan a journey" widget at /plan-a-journey. Owns its locators and interactions.
/// </summary>
public class JourneyPlannerPage(IPage page, TestSettings settings)
{
    public string Url => page.Url;

    public ILocator Heading =>
        page.GetByRole(AriaRole.Heading, new() { Name = "Plan a journey", Level = 1 });

    // Location inputs are comboboxes with visually hidden labels, e.g. "From location".
    public ILocator LocationInput(LocationField field) =>
        page.GetByRole(AriaRole.Combobox, new() { Name = $"{field} location", Exact = true });

    private ILocator Suggestion(string name) =>
        page.GetByRole(AriaRole.Option, new() { Name = name, Exact = true });

    private ILocator ChangeTimeLink =>
        page.GetByRole(AriaRole.Link, new() { Name = "change time" });

    private ILocator DepartureDate => page.GetByLabel("Date of departure");

    private ILocator DepartureTime => page.GetByLabel("Time", new() { Exact = true });

    private ILocator PlanMyJourneyButton =>
        page.GetByRole(AriaRole.Button, new() { Name = "Plan my journey" });

    // A validation message shown under a location field.
    public ILocator ValidationMessage(string text) => page.GetByText(text, new() { Exact = true });

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

    /// <summary>
    /// Types a location without choosing a suggestion, then closes the suggestion list
    /// so it won't cover the "Plan my journey" button.
    /// </summary>
    public async Task EnterLocationAsync(LocationField field, string text)
    {
        await LocationInput(field).FillAsync(text);
        await LocationInput(field).PressAsync("Escape");
    }

    /// <summary>
    /// Opens "change time" and chooses a departure date and time.
    /// </summary>
    public async Task LeaveAtAsync(DateOnly date, TimeOnly time)
    {
        await ChangeTimeLink.ClickAsync();

        // Date values: yyyyMMdd, e.g. 20261014
        // Time values: HHmm, e.g. 2030
        var dateValue = date.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var timeValue = time.ToString("HHmm", CultureInfo.InvariantCulture);
        await DepartureDate.SelectOptionAsync(dateValue);
        await DepartureTime.SelectOptionAsync(timeValue);
    }

    public async Task PlanJourneyAsync()
    {
        await PlanMyJourneyButton.ClickAsync();
    }
}
