using System.Text.RegularExpressions;

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
/// - A fresh browser context and page for each scenario;
/// - Screenshots and Playwright traces captured when a scenario fails.
/// </summary>
[Binding]
public class BrowserHooks(IObjectContainer container, ScenarioContext scenarioContext)
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
        Assertions.SetDefaultExpectTimeout(PlaywrightSettingsProvider.ExpectTimeout ?? 5_000);
        _consentStatePath = await SaveCookieConsentAsync();
    }

    [BeforeScenario]
    public async Task StartScenarioAsync()
    {
        _context = await NewContextAsync(_consentStatePath);
        await _context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true });
        var page = await _context.NewPageAsync();
        container.RegisterInstanceAs(page);
        container.RegisterInstanceAs(_settings!);
    }

    [AfterScenario]
    public async Task StopScenarioAsync()
    {
        if (_context is null)
        {
            return;
        }

        if (scenarioContext.TestError is null)
        {
            await _context.Tracing.StopAsync();
        }
        else
        {
            await SaveFailureEvidenceAsync(_context);
        }

        await _context.CloseAsync();
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

    private static async Task<IBrowserContext> NewContextAsync(string? storageStatePath)
    {
        var context = await _browser!.NewContextAsync(new BrowserNewContextOptions
        {
            StorageStatePath = storageStatePath,
        });
        context.SetDefaultNavigationTimeout(_settings!.NavigationTimeoutMs);
        return context;
    }

    private static async Task<string> SaveCookieConsentAsync()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "cookie-consent.json");
        var context = await NewContextAsync(storageStatePath: null);
        var page = await context.NewPageAsync();

        await new JourneyPlannerPage(page, _settings!).OpenAsync();
        await new CookieBanner(page).AcceptEssentialOnlyAsync();

        await context.StorageStateAsync(new BrowserContextStorageStateOptions { Path = path });
        await context.CloseAsync();
        return path;
    }

    /// <summary>
    /// Attaches a screenshot and the trace of the failed scenario to the test result.
    /// Open the trace viewer at https://trace.playwright.dev/.
    /// </summary>
    private static async Task SaveFailureEvidenceAsync(IBrowserContext context)
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "failure-evidence");
        Directory.CreateDirectory(directory);

        // The NUnit test name includes Scenario Outline values, so each gets its own files.
        var testName = Regex.Replace(TestContext.CurrentContext.Test.Name, "[^a-zA-Z0-9_-]+", "-");
        var filePath = Path.Combine(
            directory, $"{DateTime.Now:yyyyMMdd-HHmmss}-{testName.Trim('-')}");

        if (context.Pages.Count > 0)
        {
            var options = new PageScreenshotOptions { Path = $"{filePath}.png", FullPage = true };
            await context.Pages[0].ScreenshotAsync(options);
            TestContext.AddTestAttachment($"{filePath}.png", "Screenshot of the failed test");
        }

        await context.Tracing.StopAsync(new() { Path = $"{filePath}.zip" });
        TestContext.AddTestAttachment($"{filePath}.zip", "Trace of the failed test");
    }
}
