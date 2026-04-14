# MeddlingIdiot.HOS.Auditor

[![Build](https://github.com/meddlingidiot/MeddlingIdiot.HOS.Auditor/actions/workflows/build.yml/badge.svg)](https://github.com/meddlingidiot/MeddlingIdiot.HOS.Auditor/actions/workflows/build.yml)
[![GitHub release (latest by date)](https://img.shields.io/github/v/release/meddlingidiot/MeddlingIdiot.HOS.Auditor)](https://github.com/meddlingidiot/MeddlingIdiot.HOS.Auditor/releases)
[![License](https://img.shields.io/github/license/meddlingidiot/MeddlingIdiot.HOS.Auditor)](LICENSE)

A .NET library for auditing commercial driver compliance with FMCSA Hours of Service (HOS) regulations. Given a driver's duty status timeline, it detects violations across multiple US regulatory frameworks and returns structured violation results.

## Craftsmanship Note

100% of the production code in this library was hand-written — no vibe coding. The codebase was developed test-first using TDD. I'll admit that AI helped fill in some of the coverage gaps in the test suite, but the implementation itself was not generated.

## Overview

The auditor processes a driver's `ITimelineNavigator` — a moment-based duty status history — and evaluates it against configurable rule definitions. It enforces the four core FMCSA HOS rules:

| Rule | Limit |
|---|---|
| Unbroken Driving | 8 hours consecutive without a 30-minute break |
| Daily Driving | 11 hours driving after 10 consecutive hours off |
| Shift | 14-hour on-duty window (extendable to 16 with adverse conditions) |
| Weekly Window | 60 hours in 7 days / 70 hours in 8 days |

It also handles sleeper berth split provisions, adverse conditions extensions, agricultural exceptions, and shift extension overuse detection.

## Supported Rulesets

| Class | Regulation |
|---|---|
| `Us60HrRuleDefinition` | US 60-hour / 7-day (default) |
| `Us70HrRuleDefinition` | US 70-hour / 8-day |
| `UsBus60HrRuleDefinition` | US Bus carrier 60-hour / 7-day |
| `UsOilfield60HrRuleDefinition` | US Oilfield 60-hour / 7-day |

## Target Frameworks

- .NET 8.0
- .NET 9.0
- .NET 10.0

## Installation

The package is published to an internal Azure Artifacts NuGet feed. Add the feed to your `nuget.config` and install the package:

```
dotnet add package MeddlingIdiot.HOS.Auditor
```

## Getting Started

### Dependency Injection

Register the auditor with the default US 60-hour ruleset:

```csharp
services.AddAuditor();
```

This registers `IHosAuditor` and `IGpsStreamToDutyStatusTimeline` as singletons, and wires up the CQRS dispatcher handlers.

To use a different ruleset, instantiate `HosAuditor` directly:

```csharp
services.AddSingleton<IHosAuditor>(new HosAuditor(new Us70HrRuleDefinition()));
```

### Auditing a Range

Via `IHosAuditor` directly:

```csharp
var query = new AuditRangeQuery(
    startTimestamp: DateTime.UtcNow.AddDays(-7),
    finishTimestamp: DateTime.UtcNow,
    navigator: driverTimeline
);

ViolationResults results = await auditor.AuditRangeAsync(query);

foreach (var violation in results.Violations)
{
    Console.WriteLine($"{violation.ViolationType}: {violation.Comment}");
    Console.WriteLine($"  Started: {violation.StartTimestamp}, Duration: {violation.TotalSize}");
}
```

Via `IDispatcher` / `ISender`:

```csharp
var query = new AuditRangeQuery(
    startTimestamp: DateTime.UtcNow.AddDays(-7),
    finishTimestamp: DateTime.UtcNow,
    navigator: driverTimeline
);

ViolationResults results = await dispatcher.Send(query);
```

### Auditing a Point in Time

Via `IHosAuditor` directly:

```csharp
var query = new AuditPointQuery(
    timestamp: DateTime.UtcNow,
    navigator: driverTimeline
);

ViolationResults results = await auditor.AuditPointAsync(query);
```

Via `IDispatcher` / `ISender`:

```csharp
var query = new AuditPointQuery(
    timestamp: DateTime.UtcNow,
    navigator: driverTimeline
);

ViolationResults results = await dispatcher.Send(query);
```

### Targeting Specific Rules

By default, all four rules are audited. To audit only specific rules:

```csharp
var query = new AuditRangeQuery(
    startTimestamp, finishTimestamp, navigator,
    rulesToAudit: AuditRules.DrivingRules   // Only check daily driving limit
);
```

Available presets: `AuditRules.AllRules`, `AuditRules.DrivingRules`, `AuditRules.ShiftRules`, `AuditRules.WindowRules`, `AuditRules.UnbrokenDrivingRules`.

### Enabling Debug Info

```csharp
var query = new AuditRangeQuery(
    startTimestamp, finishTimestamp, navigator,
    includeDebugInfo: true
);

ViolationResults results = await dispatcher.Send(query);
Console.WriteLine(results.DebugInfo);
```

### Using the Dispatcher

`AddAuditor()` automatically calls `AddDispatcher(...)` and registers the `AuditRangeHandler` and `AuditPointHandler`. Inject `IDispatcher` (or the narrower `ISender`) wherever you need to dispatch queries:

```csharp
public class MyService(ISender sender)
{
    public async Task<ViolationResults> CheckDriverAsync(
        ITimelineNavigator timeline, CancellationToken ct)
    {
        var query = new AuditRangeQuery(
            startTimestamp: DateTime.UtcNow.AddDays(-7),
            finishTimestamp: DateTime.UtcNow,
            navigator: timeline
        );

        return await sender.Send(query, ct);
    }
}
```

### Adding Pipeline Behaviors

Register cross-cutting concerns (logging, validation, timing) as open-generic `IPipelineBehavior<TRequest, TResponse>` implementations. Call `AddOpenBehavior` after `AddAuditor`:

```csharp
services.AddAuditor();
services.AddOpenBehavior(typeof(LoggingBehavior<,>));
```

```csharp
public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling {Request}", typeof(TRequest).Name);
        var response = await next(cancellationToken);
        logger.LogInformation("Handled {Request}", typeof(TRequest).Name);
        return response;
    }
}
```

Behaviors run in registration order, wrapping the handler like middleware.

### GPS to Duty Status Conversion

If you have GPS data rather than a structured duty status timeline, convert it first:

```csharp
ITimelineNavigator dutyStatusTimeline =
    gpsConverter.ConvertGpsTimelineToDutyStatusTimeline(gpsNavigator);
```

The converter uses a 0.02-mile distance threshold to classify moments as driving or sleeping.

## Violation Results

`ViolationResults` contains:

| Property | Description |
|---|---|
| `Violations` | `List<Violation>` — all detected violations |
| `ClearViolationRange` | `ClearViolationRange` — safe date range to clear violations |
| `DebugInfo` | Optional log output (only populated when `includeDebugInfo: true`) |

Each `Violation` includes:

| Property | Description |
|---|---|
| `ViolationType` | `HoursOfService`, `GPS`, or `Manual` |
| `Comment` | Human-readable description of the violation |
| `StartTimestamp` | When the violation period began |
| `TotalSize` | Duration of the violation period |
| `OverLimitStartTime` | When the driver first went over the limit |
| `OverLimitTotalSize` | How far over the limit the driver went |
| `Limit` | The limit that was exceeded |
| `TimeInViolation` | Time spent in violation |

## Architecture

The auditor uses a rule engine pipeline:

```
ITimelineNavigator (duty status timeline)
        |
        v
  HosAuditor.AuditRange / AuditPoint
        |
        +-- Build rest period timeline
        |   (identifies qualified rest, pairs sleeper splits)
        |
        +-- SleeperSplitRuleLoop
        |   Evaluates: UnbrokenDriving, Driving, Shift, OnDuty
        |   Triggers at: duty status changes, rest accumulation, end of window
        |
        +-- DailyRuleLoop
        |   Evaluates: Window (60/70 hr rolling)
        |   Triggers at: day boundaries, rest accumulation, end of window
        |
        +-- ShiftExtensionOveruseChecker
        |   Validates extensions not overused within 7-day periods
        |
        +-- ViolationGateway (deduplication)
        |
        v
  ViolationResults
```

### Key Design Patterns

- **CQRS** — `AuditRangeQuery` / `AuditPointQuery` dispatched to handlers via `MeddlingIdiot.Dispatcher`
- **Strategy** — `IRuleDefinition` implementations swap regulatory limits without changing the engine
- **Template Method** — `RuleBase.Accumulate()` defined by subclasses (`StandardRule`, `UnbrokenRule`, `ShiftRule`, `WindowRule`)
- **Repository** — `ViolationGateway` deduplicates violations by start timestamp and limit

## FMCSA Compliance Test Coverage

All 30 examples from the official FMCSA HOS document ([FMCSA-HOS-395-HOS-EXAMPLES, 2022-04-28](https://www.fmcsa.dot.gov/sites/fmcsa.dot.gov/files/2022-04/FMCSA-HOS-395-HOS-EXAMPLES%282022-04-28%29_0.pdf)) are covered by dedicated test methods in [`FmcsaExamplesTests.cs`](MeddlingIdiot.HOS.Auditor.UnitTests/FmcsaExamplesTests.cs). Each example has its own test:

| Test | Description |
|---|---|
| `Example01_14HourDrivingWindow_NoViolations` | 14-hour driving window — no violations |
| `Example02_10HourConsecutiveHourOffDutyBreak_NoViolations` | 10-hour consecutive off-duty break — no violations |
| `Example03_DrivingLimit_NoViolations` | Driving limit — no violations |
| `Example04_DrivingLimit_WithViolations` | Driving limit — with violations |
| `Example05_RestBreaks_NoViolations` | Rest breaks — no violations |
| `Example06_34HourRestart_NoViolations` | 34-hour restart — no violations |
| `Example07_34HourRestartMixedRest_NoViolations` | 34-hour restart with mixed rest — no violations |
| `Example08_34HourRestartMultiday_NoViolations` | 34-hour restart multi-day — no violations |
| `Example09_16HourDrivingWindow_NoViolations` | 16-hour driving window — no violations |
| `Example10_16HourDrivingWindow_WithViolations` | 16-hour driving window — with violations |
| `Example11_TwoDriverPropertyCarringCmv_NoViolations` | Two-driver property-carrying CMV — no violations |
| `Example12_SleeperBerthUse_NoViolations` | Sleeper berth use — no violations |
| `Example13_TwoDriverProperCarryingCmv_WithViolations` | Two-driver property-carrying CMV — with violations |
| `Example14_SleeperBerthUseWitRestBreak_WithViolations` | Sleeper berth use with rest break — with violations |
| `Example15_SleeperBerthUseNotValidSplit_WithViolations` | Sleeper berth — invalid split — with violations |
| `Example16_SleeperBerthProperUse_NoViolations` | Sleeper berth proper use — no violations |
| `Example17_SleeperBerthPairWithFullRestSleeperSegment_NoViolations` | Sleeper berth pair with full rest segment — no violations |
| `Example18_SleeperBerthMultiplePairings_NoViolations` | Sleeper berth multiple pairings — no violations |
| `Example19_WaitingTimeAtWellSiteExample1_NoViolations` | Waiting time at well site (example 1) — no violations |
| `Example20_WaitingTimeAtWellSiteExample2_NoViolations` | Waiting time at well site (example 2) — no violations |
| `Example21_SplitBreakWithWellTime_WithViolations` | Split break with well time — with violations |
| `Example22_OilfieldWithWellWaitTime_NoViolations` | Oilfield with well wait time — no violations |
| `Example23_AgriculturalOperationException_WithViolations` | Agricultural operation exception — with violations |
| `Example24_AgriculturalOperationException_NoViolations` | Agricultural operation exception — no violations |
| `Example25_PassengerCarryingVehicles_NoViolations` | Passenger-carrying vehicles — no violations |
| `Example26_PassengerCarryingVehicles_NoViolations` | Passenger-carrying vehicles — no violations |
| `Example27_PassengerCarryingVehicles_WithViolations` | Passenger-carrying vehicles — with violations |
| `Example28_PassengerCarryingVehicles_WithViolations` | Passenger-carrying vehicles — with violations |
| `Example29_60_70_HourRule_WithViolations` | 60/70-hour rule — with violations |
| `Example30_AdverseDrivingConditionsException_NoViolations` | Adverse driving conditions exception — no violations |

## Build

This project uses [NUKE](https://nuke.build/) for its build system. The default CI target (run by GitHub Actions) executes:

1. Clean → Restore → Compile
2. Secret scan → Unit tests → Code coverage
3. Package → Push to internal NuGet feed
4. Tag release (on `main` branch)

**Local build:**

```bash
dotnet tool restore
dotnet run --project build/_build.csproj
```

**Run tests only:**

```bash
dotnet test
```

CI is configured in `azure-pipelines.yml` and runs on pushes to `main`, `feature/*`, and `bugfix/*` branches.

## Dependencies

| Package | Version | Purpose |
|---|---------|---|
| `MeddlingIdiot.Dispatcher` | —       | CQRS request/handler dispatch |
| `MeddlingIdiot.HOS.TimelineNavigator` | -       | Timeline navigation and moment structures |
