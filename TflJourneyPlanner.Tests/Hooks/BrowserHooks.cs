using Microsoft.Playwright;
using Microsoft.Playwright.TestAdapter;

using Reqnroll;
using Reqnroll.BoDi;

using TflJourneyPlanner.Tests.Support;

namespace TflJourneyPlanner.Tests.Hooks;

/// <summary>
/// Owns the browser lifecycle:
/// - One Playwright instance and one browser for the whole test run;
/// - A fresh browser context and page for each test.
/// </summary>
[Binding]
public class BrowserHooks(IObjectContainer container)
{
    private static TestSettings? _settings;
    private static IPlaywright? _playwright;
    private static IBrowser? _browser;

    private IBrowserContext? _context;

    [BeforeTestRun]
    public static async Task StartBrowserAsync()
    {
        _settings = TestSettings.Load();
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright[PlaywrightSettingsProvider.BrowserName]
            .LaunchAsync(PlaywrightSettingsProvider.LaunchOptions);
    }

    [BeforeScenario]
    public async Task StartScenarioAsync()
    {
        _context = await _browser!.NewContextAsync();
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
}
