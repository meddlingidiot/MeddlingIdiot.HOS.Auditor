# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased] - 1.0.26

### 📝 Other Changes

- Add `ExactTimestamp` to `RestMoment`, enhance GPS data with additional attributes, and implement projected split pairing moments for rest rule evaluation. ([eb2b746](../../commit/eb2b746))
- Pulling in similiar projects to eliminate projects ([4d4098a](../../commit/4d4098a))

## [1.0.25] - 2026-05-13

### 📝 Other Changes

- Add projected rest moments for final rest segment in audit process ([744bc38](../../commit/744bc38))

## [1.0.24] - 2026-05-13

### 📝 Other Changes

- Update ViolationResults to use RestMoments; modify GpsMoment initialization and update RestAccumulatorOptions for limit reached handling ([110075b](../../commit/110075b))

## [1.0.23] - 2026-05-13

### 📝 Other Changes

- Refactor audit query classes to remove DaySummaries and RestTargets properties; update ViolationResults to include optional parameters for day summaries and rest targets ([74fd580](../../commit/74fd580))

## [1.0.22] - 2026-05-13

### 📝 Other Changes

- Add `RestTargets` to `AuditPoint` and `AuditRange` queries for rest rule evaluation ([bc7acd8](../../commit/bc7acd8))

## [1.0.21] - 2026-05-11

### 📝 Other Changes

- Use `Violation.FormatHours` for consistent time formatting in rule descriptions ([30abea2](../../commit/30abea2))

## [1.0.20] - 2026-05-11

### 📝 Other Changes

- Make `FormatHours` method public for reuse ([fb2a1e2](../../commit/fb2a1e2))

## [1.0.19] - 2026-05-11

### 📝 Other Changes

- Update package versions and enhance `Violation.ToString` formatting with time formatting helper ([5ce9046](../../commit/5ce9046))

## [1.0.18] - 2026-05-11

### 📝 Other Changes

- Add US oilfield and bus 70-hour rule definitions ([90457d7](../../commit/90457d7))

## [1.0.17] - 2026-05-10

### 📝 Other Changes

- Fix `DailyRuleLoop` to correctly update day summaries using `DailyHours.ContainsKey` check ([1351f4b](../../commit/1351f4b))
- Fix `DailyRuleLoop` to call `SnapshotDay` before `GlobalReset` during global resets ([772dc18](../../commit/772dc18))

## [1.0.16] - 2026-05-10

### 📝 Other Changes

- Fix `WindowRule` method ordering and correct `DailyRuleLoop` logic for handling day summaries ([d2cadb1](../../commit/d2cadb1))

## [1.0.15] - 2026-05-10

### 📝 Other Changes

- Fix `DailyRuleLoop` to update day summaries when `HoursInWindow` or daily hours are non-zero ([6b7d7e1](../../commit/6b7d7e1))

## [1.0.14] - 2026-05-10

### 📝 Other Changes

- Reorder `DailyRuleLoop` conditions to handle global resets after start-of-day processing ([c1beabd](../../commit/c1beabd))

## [1.0.13] - 2026-05-10

### 📝 Other Changes

- Fix `DailyRuleLoop` to properly update day summaries with non-zero daily hours ([2f4d4f0](../../commit/2f4d4f0))

## [1.0.12] - 2026-05-10

### 📝 Other Changes

- Add `WindowLimit` and `DaysInWindow` properties to `DaySummary` class ([6fbd5a1](../../commit/6fbd5a1))

## [1.0.11] - 2026-05-10

### 📝 Other Changes

- Fix `DailyRuleLoop` to handle null snapshots and update existing day summaries correctly ([bf51f5a](../../commit/bf51f5a))

## [1.0.10] - 2026-05-09

### 📝 Other Changes

- Add `DaySummary` class and integrate snapshots into rule evaluation ([a3cbf8d](../../commit/a3cbf8d))

## [1.0.9] - 2026-05-07

### 📝 Other Changes

- Update dependencies to latest versions ([12c0525](../../commit/12c0525))

## [1.0.8] - 2026-05-02

### 📝 Other Changes

- Fix `Violation` calculation to use correct time intervals ([d56f05e](../../commit/d56f05e))

## [1.0.7] - 2026-05-02

### 📝 Other Changes

- Fix `Violation` constructor call to correctly calculate time in violation ([9e8fb68](../../commit/9e8fb68))

## [1.0.6] - 2026-04-24

### 📝 Other Changes

- Add .NET and C# version badges to README ([38586b9](../../commit/38586b9))

## [1.0.5] - 2026-04-18

### 📝 Other Changes

- Add `ICreateGitHubRelease` interface to `Build` class ([2a54d11](../../commit/2a54d11))

## [1.0.4] - 2026-04-18

### 📝 Other Changes

- Update dependencies across projects to latest versions ([d1cf0e3](../../commit/d1cf0e3))

## [1.0.3] - 2026-04-16

### 📝 Other Changes

- Update dependencies and add Codecov integration ([2891868](../../commit/2891868))

## [1.0.2] - 2026-04-16

### 📝 Other Changes

- Add `CancellationToken` support to audit processes and loops to improve task cancellation handling (#1) ([012a2d6](../../commit/012a2d6))

## [1.0.1] - 2026-04-14

### 📝 Other Changes

- Add FMCSA compliance test coverage details to README ([b6319d0](../../commit/b6319d0))

## [1.0.0] - 2026-04-14

No changes recorded.

## [0.0.10] - 2026-04-14

### 📝 Other Changes

- Update badges in README to reference `HOS.Auditor` project ([6d6ee16](../../commit/6d6ee16))

## [0.0.9] - 2026-04-14

### 📝 Other Changes

- Add build, release, and license badges to README ([24e4b43](../../commit/24e4b43))

## [0.0.8] - 2026-04-12

### 📝 Other Changes

- Add Apache 2.0 license to repository ([08322c1](../../commit/08322c1))

## [0.0.7] - 2026-04-12

### 📝 Other Changes

- Add craftsmanship note to README and fix formatting in dependencies table ([d84a540](../../commit/d84a540))

## [0.0.6] - 2026-04-12

### 📝 Other Changes

- Remove `MeddlingIdiot.Dispatcher` project from solution and update changelog for v0.0.5. ([5a594bc](../../commit/5a594bc))
- Update `Dispatcher` and `TimelineNavigator` to v0.0.6, adjust namespaces, and enhance documentation. ([32855e7](../../commit/32855e7))

## [0.0.5] - 2026-04-12

### 📝 Other Changes

- update namespaces, restructure solution, and update dependencies ([0911406](../../commit/0911406))

## [0.0.4] - 2026-04-11

### 📝 Other Changes

- Update TimelineNavigator to v0.0.4, enhance solution structure, and improve documentation ([62c83ed](../../commit/62c83ed))

## [0.0.3] - 2026-04-11

### 📝 Other Changes

- Got TUnit tests working! ([91ecd22](../../commit/91ecd22))

## [0.0.2] - 2026-04-10

### 📝 Other Changes

- Rename `DataConverter` to `GpsStreamToDutyStatusTimeline`, update interface and DI registration, and add unit tests ([c2f8df9](../../commit/c2f8df9))

## [0.0.1] - 2026-04-10

### 📝 Other Changes

- Comment out unused logging in `RestTimelinePairerUsaPrimaryShould.BuildNormalHappyPath` test ([c25917f](../../commit/c25917f))
- Add coverlet.collector package to UnitTests project for code coverage ([aac63aa](../../commit/aac63aa))
- initial commit here.. ([89a24d5](../../commit/89a24d5))

