# TfL Journey Planner UI tests

Automated UI tests for the TfL website – "Plan a journey" widget (the "Journey Planner") at https://tfl.gov.uk/plan-a-journey/, written in C# with Gherkin and Playwright.

## Tech stack

<table>
    <tr>
        <td><strong>Language and runtime</strong></td>
        <td>C# on .NET 10.0 (LTS)</td>
    </tr>
    <tr>
        <td><strong>BDD</strong></td>
        <td>Gherkin syntax, Reqnroll 3.3 with NUnit 4</td>
    </tr>
    <tr>
        <td><strong>Browser automation</strong></td>
        <td>Playwright for .NET 1.63, driving Google Chrome</td>
    </tr>
    <tr>
        <td><strong>Configuration</strong></td>
        <td><code>test.runsettings</code></td>
    </tr>
</table>

## Running the tests

### Prerequisites

> [!NOTE]
> The tests drive Google Chrome installed on your machine, so there's no Playwright browser download.

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (any 10.0.x)
- Google Chrome

### Commands

From the repository root, run:

```bash
dotnet test
```

The tests run without a visible browser. To watch the run:

```bash
HEADED=1 dotnet test        // Mac and Linus
$env:HEADED=1; dotnet test  // Windows PowerShell
```

Run by tag:

```bash
dotnet test --filter Category=smoke
dotnet test --filter Category=negative
```

## Project structure

```
tfl-journey-planner-tests/
├── .github/workflows/test.yml            GitHub Actions workflow
├── TflJourneyPlanner.Tests/
│   ├── Features/JourneyPlanner.feature   The 5 brief scenarios plus a smoke test, in Gherkin
│   ├── Pages/                            Page objects: own every locator and interaction
│   │   ├── JourneyPlannerPage.cs
│   │   ├── JourneyResultsPage.cs
│   │   └── CookieBanner.cs
│   ├── StepDefinitions/                  Thin steps: call a page object and assert
│   │   ├── JourneyPlannerSteps.cs
│   │   └── JourneyResultsSteps.cs
│   ├── Hooks/BrowserHooks.cs             Browser lifecycle and cookie consent
│   ├── Support/                          Plain C# helpers with no Playwright dependency
│   │   ├── TestSettings.cs               Typed settings from test.runsettings
│   │   ├── JourneyTimeParser.cs          Parses journey times into whole minutes
│   │   ├── TravelDates.cs                The next weekday, in London time
│   │   └── LocationField.cs              The From and To fields
│   ├── reqnroll.json                     Makes undefined steps fail the run
│   └── test.runsettings                  Browser options and the base URL
├── .editorconfig                         Code style rules, checked on build
├── global.json                           Pins the .NET 10 SDK
└── TflJourneyPlanner.slnx
```

## Scenarios

| #   | Brief                                                        | Scenario                                                                                                                                     | Tags        |
| --- | ------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------- | ----------- |
| -   | (smoke test)                                                 | [The Journey Planner loads](/TflJourneyPlanner.Tests/Features/JourneyPlanner.feature#L8-L10)                                                 | `@smoke`    |
| 1   | Plan a valid journey; validate walking and cycling times     | [Plan a journey between two stations chosen from autocomplete suggestions](/TflJourneyPlanner.Tests/Features/JourneyPlanner.feature#L14-L20) | `@smoke`    |
| 2   | Edit preferences to least walking; validate the journey time | [Show routes with least walking](/TflJourneyPlanner.Tests/Features/JourneyPlanner.feature#L54-L58)                                           |             |
| 3   | View details; verify complete access information             | [View complete access information for a station](/TflJourneyPlanner.Tests/Features/JourneyPlanner.feature#L60-L67)                           |             |
| 4   | No results for an invalid journey                            | [No results are shown when an invalid journey is planned](/TflJourneyPlanner.Tests/Features/JourneyPlanner.feature#L22-L34)                  | `@negative` |
| 5   | No journey without locations                                 | [A journey cannot be planned if no locations are entered](/TflJourneyPlanner.Tests/Features/JourneyPlanner.feature#L36-L41)                  | `@negative` |

## Development decisions

### Playwright, driving installed Google Chrome

The challenge brief names "Playwright" and "Chromedriver". Playwright controls Chrome directly and doesn't use ChromeDriver, which is Selenium's driver. I took the intent of this challenge to be testing in real Google Chrome (`<Channel>chrome</Channel>`) without adding a second UI tool. If ChromeDriver itself were required, I'd add a Selenium smoke test alongside.

### Reqnroll with NUnit

> [!NOTE]
> Reqnroll is the maintained successor to SpecFlow, which reached end of life at the end of 2024.

Reqnroll runs the Gherkin scenarios on NUnit, which turns Gherkin tags into test categories, so runs can be filtered with `--filter Category=smoke`. NUnit is pinned to 4.6.1, the version the suite was built with. A trial upgrade to NUnit 5 passed, but as a new major version I'd upgrade it only when the suite is stable.

### Reqnroll hooks own the browser, not `Microsoft.Playwright.NUnit`

Playwright's NUnit package provides `PageTest` base classes that open the browser in NUnit's `[SetUp]`, but Reqnroll creates its own step classes, so that setup never runs for them. Instead, `BrowserHooks` starts one browser for the whole run, gives each scenario a fresh context and page, and registers `IPage` and `TestSettings` in Reqnroll's container.

### Page objects, thin steps, behaviour-only Gherkin

Feature files describe behaviour only, in plain English, with no selectors. Steps are a few lines each: they call a page object, then assert. Page objects own every locator, perform actions and return data, but never assert.

### Locators: roles first

`GetByRole` and `GetByLabel` find elements the way a user, or a screen reader, does, and survive changes to markup, classes and IDs. CSS is used only where the page offers no accessible hook, and each case has a comment explaining why. For those few elements, I'd suggest the development team add `data-testid` attributes, which Playwright's `GetByTestId` uses, to make the locators more robust.

### Two kinds of assertions, no fixed waits

Playwright's `Expect(...)` is used for anything on screen, because it retries until the page catches up. NUnit's `Assert.That` is used for values already read and parsed, such as journey minutes. There's deliberately no `Task.Delay` or `Thread.Sleep` anywhere.

### Accessibility checked

Scenario 5 checks that each "field is required" message is in the field's accessible description, so a screen reader announces it, not only that it appears on screen. It's a small example of building accessibility checks into functional tests.

### Cookie consent given once per run

Before the first scenario, the hooks open the Journey Planner, choose "Accept only essential cookies", wait for the consent cookie and save the browser's storage state. Every scenario starts from that state, so the banner never appears. Playwright suites use the same pattern to reuse a signed-in session, so tests of authenticated pages wouldn't need to sign in each time. The banner depends on the visitor's region, so the setup continues if no cookie banner appears.

### Settings in `test.runsettings`

It's the test runner's own settings file, and the `.csproj` sets it as `RunSettingsFilePath`, so a plain `dotnet test` uses it. Browser options use Playwright's documented `<Playwright>` section, so Playwright's usual overrides work, such as `HEADED=1`. The base URL and navigation timeout are NUnit test run parameters, required in code, so a missing one stops the run with a clear message.

### Settings added when they were needed

Rather than configuring everything up front, I added settings when the live site showed a real need. The 60-second navigation timeout and 15-second assertion timeout came after the site was seen taking longer than Playwright's 30-second and 5-second defaults.

### Evidence when a scenario fails

Every scenario records a Playwright trace. When a scenario fails, a full-page screenshot and the trace are saved to `TflJourneyPlanner.Tests/bin/Debug/net10.0/failure-evidence/` and attached to the test result. Open a trace at https://trace.playwright.dev.

### A smoke test that checks the page heading

The Journey Planner page was sometimes served blank with HTTP 200 (see [Issues found](#issues-found)), so the smoke test checks the page heading is visible, not just that the page responded.

### Undefined steps fail the run

`reqnroll.json` makes a step with no matching definition an error. By default, it's only reported as "not executed", which the test summary doesn't count. I turned this on once all the steps were written, so a misspelt step can't go unnoticed.

### Built to scale, on purpose

Some patterns in this project are more than a single feature file strictly needs:

- a `LocationField` enum, so an invalid field name in the Gherkin fails straight away rather than timing out
- a time parser with no Playwright dependency, so it could be unit tested
- a typed `TestSettings` class
- separate page objects and step classes

I implemented and kept them to show how the framework would grow on a larger project.

### Code style enforced

The project uses the standard .NET `.editorconfig` (with final newlines), checks code-style rules on build, and runs `dotnet format --verify-no-changes` before each commit.

## Additional scenarios

Scenarios I'd test next for the Journey Planner. They are listed below and aren't automated in this project. Several come from things found while building this suite (see [Issues found](#issues-found)).

### Functional

- **Leaving and arriving times:** leaving-at and arrive-by journeys, a time in the past (the results show a reminder), and the furthest date the date list allows.

- **Other kinds of locations:** postcodes, street addresses and points of interest, chosen from the suggestions, plus "Use my location" with the browser's location permission allowed and denied.

- **Every "Show me" preference:** fastest, fewest changes and least walking each change the results and the summary text. Scenario 2 covers least walking only.

- **Every journey option shows a valid time**, not just the first one that scenario 2 checks.

- **Access options:** for example "Full step-free access" and "Step-free to platform only" return only routes whose steps match, and "Use stairs, not escalators" avoids escalators.

- **The same place for From and To:** a place chosen in From isn't offered again in the To suggestions.

- **Error messages are consistent:** different invalid inputs currently give two different "no results" messages.

- **Recent searches and favourites** are remembered and can be reused (when cookies are allowed).

### Non-functional

- **Accessibility** (WCAG 2.2 AA):
  - an automated axe scan of the Journey Planner and results page
  - the whole journey with keyboard navigation only
  - colour contrast and page zoom

- **Performance:** how quickly suggestions appear while typing, and the time from "Plan my journey" to results; load testing of the journey planner API with JMeter or k6.

- **Compatibility:** Firefox, Safari (WebKit) and Edge, and mobile viewports with touch. The tests could run on other browsers and viewports through the `<Playwright>` section of `test.runsettings`.

- **Security:** script and markup injection in the From and To fields is shown as text and never runs.

- **Privacy:** after "Accept only essential cookies", no non-essential cookies or trackers are set.

## Issues found

Things I found while building the suite, which I'd raise with the team. Each one may affect real users, not just automated tests.

1. **The Journey Planner page was sometimes served blank.** Without the trailing slash, `https://tfl.gov.uk/plan-a-journey` should redirect to `/plan-a-journey/?cid=plan-a-journey`, and usually does. During the exploration stage of this project (on 12 Oct), though, some `curl` requests got HTTP 200 with an empty page instead, and the smoke test failed 2 runs in 5. It looks like a cached bad response that has since expired: all requests redirected correctly in a later check (on 15 Oct). The tests avoid it by opening `/plan-a-journey/` directly.

1. **Declining cookies too quickly leaves the page unusable.** Choosing "Accept only essential cookies" while the banner is still entering the viewport leaves a full-screen overlay that blocks every click until the page is refreshed. A fast user could hit this. The tests avoid it by giving consent once per run (see [Cookie consent given once per run](#cookie-consent-given-once-per-run)).

1. **"No results" messages are inconsistent.** `#####` as a location gives "Journey planner could not find any results to your search. Please try again", while `@@@@@` gives "Sorry, we can't find a journey matching your criteria".

1. **(Accessibility) The access information icons are links that go nowhere.** In the journey details, each access icon ("Up stairs", "Up lift" and so on) is an `<a href="javascript:void(0);">` with an `aria-label`. Screen readers announce each one as a link, but activating one does nothing. Since they're information, not actions, they should be images with alternative text or a list[^1].

[^1]: WCAG [4.1.2 Name, Role, Value](https://www.w3.org/WAI/WCAG22/Understanding/name-role-value.html), and [2.4.4 Link Purpose](https://www.w3.org/WAI/WCAG22/Understanding/link-purpose-in-context.html)

## Continuous integration

[.github/workflows/test.yml](.github/workflows/test.yml) runs the suite on GitHub Actions: Ubuntu, .NET 10 and headless Google Chrome, which is preinstalled on GitHub's runners. When a test fails, it uploads the test results and the failure evidence (screenshots and traces) as an artifact.

**It's set to run manually only.** On GitHub-hosted runners, tfl.gov.uk shows a Cloudflare verification check, which stops the tests ([example run with screenshots](https://github.com/davelyo/tfl-journey-planner-tests/actions/runs/38766915895)). I didn't try to get around the site protection in this project. In a real team, the suite would run from an allow-listed or self-hosted build agent.

Running on GitHub also showed that the cookie banner depends on the visitor's region, so the setup now carries on when no banner appears (see [Cookie consent given once per run](#cookie-consent-given-once-per-run)).

## Use of AI and other resources

> [!NOTE]
> This section was drafted with Claude Code based on a review of the development process and activities throughout this challenge, and subsequently revised by me.

> **Disclaimer**
>
> No AI-generated code was used as a direct solution to the coding challenge. However, GitHub Copilot was enabled for inline code suggestions during development and may have suggested or completed code based on patterns learned from publicly available and community-contributed code. AI-assisted reviews and recommendations were used as supplementary guidance, with responsibility for the final implementation and development decisions remaining with me.

AI tools and external resources were used to support the learning, research, planning, and development process for this coding challenge.

### AI tools

- **Claude and Claude Code:** Used to proofread this README and code comments; research the technology stack for Playwright with .NET, C#, and Gherkin; discuss and evaluate development decisions; review changes after each commit for potential bugs, formatting issues, and spelling errors; and develop a C# and .NET refresher and learning plan, including explanations of relevant concepts through comparisons with TypeScript.

- **GitHub Copilot:** Used for inline code suggestions while developing in Visual Studio Code.

### Technical references

- [⁠Playwright for .NET](https://playwright.dev/dotnet/docs/intro)
- [⁠.NET SDK and runtime versions](https://versionsof.net/core/10.0/)
- [⁠Microsoft Learn — .NET](https://dotnet.microsoft.com/en-us/learn)
- [⁠Reqnroll with Playwright and C#](https://github.com/rohitsaraswat7865/Reqnroll_Playwright_CSharp)

### Development tools and Visual Studio Code extensions

The following extensions helped with environment setup, C# development, and Gherkin syntax support:

- [⁠.NET Install Tool](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.vscode-dotnet-runtime)
- [⁠C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)
- [⁠C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)
- [⁠Feature Syntax Highlight and Snippets (Cucumber/Gherkin)](https://marketplace.visualstudio.com/items?itemName=Blodwynn.featurehighlight)
