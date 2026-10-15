# TfL Journey Planner UI tests

Automated UI tests for the TfL website – "Plan a journey" widget (the "Journey Planner") at https://tfl.gov.uk/plan-a-journey/, written in C# with Gherkin and Playwright.

## Tech stack

|                          |                                                 |
| ------------------------ | ----------------------------------------------- |
| **Language and runtime** | C# on .NET 10.0 (LTS)                           |
| **BDD**                  | Gherkin syntax, Reqnroll 3.3 with NUnit 4       |
| **Browser automation**   | Playwright for .NET 1.63, driving Google Chrome |
| **Configuration**        | `test.runsettings`                              |

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
HEADED=1 dotnet test
```

Run by tag:

```bash
dotnet test --filter Category=smoke
dotnet test --filter Category=negative
```

## Project structure

```
TflJourneyPlanner.Tests/
├── Features/JourneyPlanner.feature   The 5 brief scenarios plus a smoke test, in Gherkin
├── Pages/                            Page objects: own every locator and interaction
│   ├── JourneyPlannerPage.cs
│   ├── JourneyResultsPage.cs
│   └── CookieBanner.cs
├── StepDefinitions/                  Thin steps: call a page object and assert
│   ├── JourneyPlannerSteps.cs
│   └── JourneyResultsSteps.cs
├── Hooks/BrowserHooks.cs             Browser lifecycle and cookie consent
├── Support/                          Plain C# helpers with no Playwright dependency
│   ├── TestSettings.cs               Typed settings from test.runsettings
│   ├── JourneyTimeParser.cs          Parses journey times into whole minutes
│   ├── TravelDates.cs                The next weekday, in London time
│   └── LocationField.cs              The From and To fields
└── test.runsettings                  Browser options and the base URL
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

<!-- ## Development decisions -->

<!-- ## Additional scenarios -->

<!-- ### Functional -->

<!-- ### Non-functional -->

<!-- ## Use of AI -->
