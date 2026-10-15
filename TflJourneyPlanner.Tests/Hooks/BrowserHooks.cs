using Microsoft.Playwright;
using Microsoft.Playwright.TestAdapter;

using Reqnroll;
using Reqnroll.BoDi;

using TflJourneyPlanner.Tests.Pages;
using TflJourneyPlanner.Tests.Support;

namespace TflJourneyPlanner.Tests.Hooks;

/// <summary>
/// Owns the browser lifecycle:
/// - One Playwright instance and one browser for the whole test run;
/// - Cookie consent (essential cookies only) is given once per test run and reused;
/// - A fresh browser context and page for each scenario.
/// </summary>
[Binding]
public class BrowserHooks(IObjectContainer container)
{
    private static TestSettings? _settings;
    private static IPlaywright? _playwright;
    private static IBrowser? _browser;
    private static string? _consentStatePath;

    private IBrowserContext? _context;

    [BeforeTestRun]
    public static async Task StartBrowserAsync()
    {
        _settings = TestSettings.Load();
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright[PlaywrightSettingsProvider.BrowserName]
            .LaunchAsync(PlaywrightSettingsProvider.LaunchOptions);
        _consentStatePath = await SaveCookieConsentAsync();
    }

    [BeforeScenario]
    public async Task StartScenarioAsync()
    {
        _context = await _browser!.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = _consentStatePath,
        });
        var page = await _context.NewPageAsync();
        container.RegisterInstanceAs(page);
        container.RegisterInstanceAs(_settings!);
    }

    [AfterScenario]
    public async Task StopScenarioAsync()
    {
        if (_context is not null)
        {
            await _context.CloseAsync();
        }
    }

    [AfterTestRun]
    public static async Task StopBrowserAsync()
    {
        if (_browser is not null)
        {
            await _browser.CloseAsync();
        }

        _playwright?.Dispose();
    }

    private static async Task<string> SaveCookieConsentAsync()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "cookie-consent.json");
        var context = await _browser!.NewContextAsync();
        var page = await context.NewPageAsync();

        await new JourneyPlannerPage(page, _settings!).OpenAsync();
        await new CookieBanner(page).AcceptEssentialOnlyAsync();

        await context.StorageStateAsync(new BrowserContextStorageStateOptions { Path = path });
        await context.CloseAsync();
        return path;
    }
}
