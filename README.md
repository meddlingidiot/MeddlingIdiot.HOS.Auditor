# Automation.HOS.Auditor

A .NET library for auditing commercial driver compliance with FMCSA Hours of Service (HOS) regulations. Given a driver's duty status timeline, it detects violations across multiple US regulatory frameworks and returns structured violation results.

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
dotnet add package Automation.HOS.Auditor
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

```csharp
var query = new AuditRangeQuery(
    startTimestamp: DateTime.UtcNow.AddDays(-7),
    finishTimestamp: DateTime.UtcNow,
    navigator: driverTimeline
);

ViolationResults results = auditor.AuditRange(query);

foreach (var violation in results.Violations)
{
    Console.WriteLine($"{violation.ViolationType}: {violation.Comment}");
    Console.WriteLine($"  Started: {violation.StartTimestamp}, Duration: {violation.TotalSize}");
}
```

### Auditing a Point in Time

```csharp
var query = new AuditPointQuery(
    timestamp: DateTime.UtcNow,
    navigator: driverTimeline
);

ViolationResults results = auditor.AuditPoint(query);
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

ViolationResults results = auditor.AuditRange(query);
Console.WriteLine(results.DebugInfo);
```

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

- **CQRS** — `AuditRangeQuery` / `AuditPointQuery` dispatched to handlers via `Automation.Dispatcher`
- **Strategy** — `IRuleDefinition` implementations swap regulatory limits without changing the engine
- **Template Method** — `RuleBase.Accumulate()` defined by subclasses (`StandardRule`, `UnbrokenRule`, `ShiftRule`, `WindowRule`)
- **Repository** — `ViolationGateway` deduplicates violations by start timestamp and limit

## Build

This project uses [NUKE](https://nuke.build/) for its build system. The default CI target (run by Azure Pipelines) executes:

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
|---|---|---|
| `Automation.Dispatcher` | 0.0.2 | CQRS request/handler dispatch |
| `MeddlingIdiot.HOS.TimelineNavigator` | 0.0.4 | Timeline navigation and moment structures |
