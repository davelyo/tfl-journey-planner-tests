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

    /// <summary>
    /// Chooses "Accept only essential cookies" if the banner appears, and returns whether it did.
    /// The banner depends on the user's region and wasn't found when the tests ran on GitHub.
    /// </summary>
    public async Task<bool> AcceptEssentialOnlyIfShownAsync()
    {
        try
        {
            // Returns early if the banner is shown; otherwise waits the full timeout.
            await EssentialOnlyButton.WaitForAsync(new() { Timeout = 10_000 });
        }
        catch (TimeoutException)
        {
            return false;
        }

        await EssentialOnlyButton.ClickAsync();
        await page.WaitForFunctionAsync("() => document.cookie.includes('CookieConsent=')");
        return true;
    }
}
