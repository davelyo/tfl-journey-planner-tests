using Microsoft.Playwright;

using TflJourneyPlanner.Tests.Support;

namespace TflJourneyPlanner.Tests.Pages;

/// <summary>The journey results page. Owns its locators and interactions.</summary>
public class JourneyResultsPage(IPage page)
{
    // The "Walking" and "Cycling" boxes are links whose heading is the mode name.
    public ILocator ModeOption(string mode) =>
        page.GetByRole(AriaRole.Link).Filter(
            new() { Has = page.GetByRole(AriaRole.Heading, new() { Name = mode, Exact = true }) });

    // The header row of every journey option: public transport and (hidden) walking and cycling.
    // Each is a role="button" whose accessible name is all its text,
    // so a CSS class is the only stable hook.
    public ILocator JourneyOptions => page.Locator(".journey-option");

    // An error message shown instead of results.
    public ILocator ErrorMessage(string text) => page.GetByText(text, new() { Exact = true });

    // Summary of the chosen preference, e.g. "Showing routes with least walking".
    public ILocator PreferenceSummary(string text) => page.GetByText(text, new() { Exact = true });

    private ILocator EditPreferencesButton =>
        page.GetByRole(AriaRole.Button, new() { Name = "Edit preferences" });

    private ILocator ShowMeOptions =>
        page.GetByRole(AriaRole.Group, new() { Name = "Show me" });

    private ILocator UpdateJourneyButton =>
        page.GetByRole(AriaRole.Button, new() { Name = "Update journey" });

    // The first (recommended) journey option: its heading and its expanded content.
    private ILocator FirstOption => page.Locator("#option-1-heading");
    private ILocator FirstOptionContent => page.Locator("#option-1-content");

    // "View details" button has aria-hidden="true", so the lookup must include hidden elements.
    private ILocator ViewDetailsButton => FirstOptionContent.GetByRole(
        AriaRole.Button, new() { Name = "View details", IncludeHidden = true });

    private ILocator HideDetailsButton =>
        FirstOptionContent.GetByRole(AriaRole.Button, new() { Name = "Hide details" });

    /// <summary>
    /// Reads the time, in minutes, from one box in the "Walking and cycling" section.
    /// </summary>
    public async Task<int> GetModeMinutesAsync(string mode)
    {
        // The journey time has no accessible name, so a CSS class is the only stable hook.
        var time = ModeOption(mode).Locator(".journey-info");
        await time.WaitForAsync();
        return JourneyTimeParser.ToMinutes(await time.InnerTextAsync());
    }

    /// <summary>
    /// Reads the total time, in minutes, of the first option in the result list.
    /// Walking and cycling options use <see cref="GetModeMinutesAsync"/>.
    /// </summary>
    public async Task<int> GetFirstOptionMinutesAsync()
    {
        var time = FirstOption.Locator(".journey-time");
        await time.WaitForAsync();

        // e.g. "Total time: 11mins"
        return JourneyTimeParser.ToMinutes(await time.InnerTextAsync());
    }

    public async Task EditPreferencesAsync()
    {
        await EditPreferencesButton.ClickAsync();
    }

    /// <summary>
    /// Chooses an option under "Show me", e.g. "The fastest routes".
    /// </summary>
    public async Task ChoosePreferenceAsync(string preference)
    {
        // The actual radio button is invisible (opacity: 0) so click the visible label instead.
        await ShowMeOptions.GetByText(preference).ClickAsync();
    }

    public async Task UpdateJourneyAsync()
    {
        await UpdateJourneyButton.ClickAsync();
    }

    public async Task ViewDetailsOfFirstOptionAsync()
    {
        await ViewDetailsButton.ClickAsync();
        await HideDetailsButton.WaitForAsync();
    }

    /// <summary>
    /// Reads the access icons where each is a link whose aria-label describes the access feature.
    /// </summary>
    public async Task<List<string>> GetAccessInformationAsync(string station)
    {
        var step = FirstOptionContent.Locator(".journey-detail-step").Filter(new()
        {
            Has = page.Locator(".location-name").GetByText(station, new() { Exact = true }),
        });

        // The icons are links that go nowhere (see README, "Issues found"); update this if fixed.
        var icons = step.Locator(".access-information").GetByRole(AriaRole.Link);
        await icons.First.WaitForAsync();

        var names = new List<string>();
        foreach (var icon in await icons.AllAsync())
        {
            names.Add(await icon.GetAttributeAsync("aria-label") ?? "");
        }

        return names;
    }
}
