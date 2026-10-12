using Microsoft.Playwright;

namespace TflJourneyPlanner.Tests.Pages;

/// <summary>
/// The cookie consent banner shown on a first visit to tfl.gov.uk. Includes the minimal
/// set of interactions needed to accept essential cookies before the tests run.
/// </summary>
public class CookieBanner(IPage page)
{
    private ILocator EssentialOnlyButton =>
        page.GetByRole(AriaRole.Button, new() { Name = "Accept only essential cookies" });

    public async Task AcceptEssentialOnlyAsync()
    {
        await EssentialOnlyButton.ClickAsync();
        await page.WaitForFunctionAsync("() => document.cookie.includes('CookieConsent=')");
    }
}
