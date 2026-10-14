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

    // The header row of every journey option: public transport and (hidden) walking and cycling
    // Each is a role="button" whose accessible name is all its text,
    // so a CSS class is the only stable hook.
    public ILocator JourneyOptions => page.Locator(".journey-option");

    // An error message shown instead of results.
    public ILocator ErrorMessage(string text) => page.GetByText(text, new() { Exact = true });

    public async Task<int> GetModeMinutesAsync(string mode)
    {
        // The journey time has no accessible name, so a CSS class is the only stable hook.
        var time = ModeOption(mode).Locator(".journey-info");
        await time.WaitForAsync();
        return JourneyTimeParser.ToMinutes(await time.InnerTextAsync());
    }
}
