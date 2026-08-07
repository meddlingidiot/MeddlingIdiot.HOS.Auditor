using MeddlingIdiot.HOS.OffDutyCheckers;
using MeddlingIdiot.HOS.Queries;
using MeddlingIdiot.HOS.RestTimelineBuilders;
using MeddlingIdiot.HOS.RuleLoop;
using MeddlingIdiot.HOS.Rules;
using MeddlingIdiot.HOS.Ruleset;
using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Explorers;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;
using MeddlingIdiot.HOS.TimelineNavigator.Utilities;
using MeddlingIdiot.HOS.Violations;

namespace MeddlingIdiot.HOS
{
    public class HosAuditor : IHosAuditor
    {
        private readonly IRuleDefinition _ruleDefinition;

        public HosAuditor(IRuleDefinition ruleDefinition)
        {
            _ruleDefinition = ruleDefinition;
        }

        public Task<ViolationResults> AuditRangeAsync(AuditRangeQuery query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(AuditRange(query, cancellationToken));
        }

        public Task<ViolationResults> AuditPointAsync(AuditPointQuery query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(AuditPoint(query, cancellationToken));
        }

        public ViolationResults AuditRange(AuditRangeQuery query, CancellationToken cancellationToken = default)
        {
            //Calculate start and end of audit window
            query.Navigator.JumpTo(query.FinishTimestamp);
            var endOfAuditWindow = query.Navigator.FindRest(
                               _ruleDefinition.GlobalReset,
                                              TimelineDirection.Forward,
                                              PreferredEndOfRest.Ending,
                                              MoveTo.None);
            query.Navigator.JumpTo(query.StartTimestamp);
            var startOfAuditWindow = query.Navigator.FindRest(
                               _ruleDefinition.GlobalReset,
                                              TimelineDirection.Backward,
                                              PreferredEndOfRest.Beginning,
                                              MoveTo.NewLocation);

            return AuditNoLookBack(startOfAuditWindow, endOfAuditWindow, query.Navigator, query.Rules, query.IncludeDebugInfo, cancellationToken);
        }

        public ViolationResults AuditPoint(AuditPointQuery query, CancellationToken cancellationToken = default)
        {
            //Calculate start and end of audit window
            query.Navigator.JumpTo(query.Timestamp);
            var endOfAuditWindow = query.Navigator.FindRest(
                _ruleDefinition.GlobalReset,
                TimelineDirection.Forward,
                PreferredEndOfRest.Ending,
                MoveTo.None);
            var startOfAuditWindow = query.Navigator.FindRest(
                _ruleDefinition.GlobalReset,
                TimelineDirection.Backward,
                PreferredEndOfRest.Beginning,
                MoveTo.NewLocation);

            return AuditNoLookBack(startOfAuditWindow, endOfAuditWindow, query.Navigator, AuditRules.AllRules, query.IncludeDebugInfo, cancellationToken);

        }

        private ViolationResults AuditNoLookBack(Moment startOfAuditWindow, Moment endOfAuditWindow, ITimelineNavigator navigator, IList<AuditRule> rulesToAudit, bool includeDebugInfo, CancellationToken cancellationToken = default)
        {
            ILogger logger = new NullLogger();
            if (includeDebugInfo)
            {
                logger = new InMemoryLogger();
            }

            //The jurisdiction timeline splits the audit window into ranges. Each distinct
            //jurisdiction audits the full window with its own rule definition (so hours
            //accumulate correctly across boundaries), but may only throw violations inside
            //the ranges where it was active - jurisdiction start/end are hard limits.
            var jurisdictionRanges = BuildJurisdictionRanges(navigator, startOfAuditWindow.Timestamp, endOfAuditWindow.Timestamp, logger);
            var foreignJurisdictionNames = jurisdictionRanges
                .Where(r => r.JurisdictionName != null)
                .Select(r => r.JurisdictionName!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var allViolations = new List<Violation>();
            var ranForeignAudit = false;
            foreach (var jurisdictionName in foreignJurisdictionNames)
            {
                var ruleDefinition = JurisdictionRuleDefinitionFactory.Create(jurisdictionName)!;
                navigator.ClearRestTimeline();
                var foreignViolations = RunAudit(ruleDefinition, startOfAuditWindow, endOfAuditWindow, navigator, rulesToAudit, logger, new List<DaySummary>(), cancellationToken);
                var ownedRanges = jurisdictionRanges
                    .Where(r => string.Equals(r.JurisdictionName, jurisdictionName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                allViolations.AddRange(ClipViolationsToRanges(foreignViolations, ownedRanges));
                ranForeignAudit = true;
            }

            //The default jurisdiction runs last so the navigator's rest timeline and the
            //day summaries in the results come from the default rule definition.
            if (ranForeignAudit)
                navigator.ClearRestTimeline();
            var daySummaries = new List<DaySummary>();
            var defaultViolations = RunAudit(_ruleDefinition, startOfAuditWindow, endOfAuditWindow, navigator, rulesToAudit, logger, daySummaries, cancellationToken);
            if (ranForeignAudit)
            {
                var defaultRanges = jurisdictionRanges.Where(r => r.JurisdictionName == null).ToList();
                allViolations.AddRange(ClipViolationsToRanges(defaultViolations, defaultRanges));
            }
            else
            {
                allViolations.AddRange(defaultViolations);
            }

            allViolations.Sort((a, b) => a.StartTimestamp.CompareTo(b.StartTimestamp));

            startOfAuditWindow = DontAllowClearViolationsToStartAtBeginningOfTime(navigator, startOfAuditWindow);
            endOfAuditWindow = DontAllowClearViolationsToEndAtEndOfTime(navigator, endOfAuditWindow);
            var clearViolationRange =
                new ClearViolationRange(startOfAuditWindow.Timestamp, endOfAuditWindow.Timestamp);

            if (cancellationToken.IsCancellationRequested)
                return new ViolationResults([], clearViolationRange, logger.GetResults());

            return new ViolationResults(allViolations, clearViolationRange, logger.GetResults(), daySummaries, navigator.GetRestTimelineMoments());
        }

        private sealed record JurisdictionRange(string? JurisdictionName, DateTime Start, DateTime End);

        private List<JurisdictionRange> BuildJurisdictionRanges(ITimelineNavigator navigator, DateTime windowStart, DateTime windowEnd, ILogger logger)
        {
            var moments = navigator.GetJurisdictionMoments()
                .OrderBy(m => m.Timestamp)
                .ToList();

            var ranges = new List<JurisdictionRange>();
            string? currentName = null; //default jurisdiction until the first moment
            var currentStart = windowStart;

            foreach (var moment in moments)
            {
                if (moment.Timestamp >= windowEnd)
                    break;

                var effectiveName = ResolveJurisdictionName(moment.JurisdictionName, logger);
                if (moment.Timestamp <= windowStart)
                {
                    //A change before the window only decides which jurisdiction is active at the start.
                    currentName = effectiveName;
                    continue;
                }

                if (!string.Equals(currentName, effectiveName, StringComparison.OrdinalIgnoreCase))
                {
                    if (moment.Timestamp > currentStart)
                        ranges.Add(new JurisdictionRange(currentName, currentStart, moment.Timestamp));
                    currentName = effectiveName;
                    currentStart = moment.Timestamp;
                }
            }

            if (windowEnd > currentStart)
                ranges.Add(new JurisdictionRange(currentName, currentStart, windowEnd));

            return ranges;
        }

        private string? ResolveJurisdictionName(string? jurisdictionName, ILogger logger)
        {
            if (jurisdictionName == null)
                return null;
            if (JurisdictionRuleDefinitionFactory.IsKnown(jurisdictionName))
                return jurisdictionName;

            logger.Debug(LoggerCategories.Rule, $"Unknown jurisdiction '{jurisdictionName}' - falling back to the default jurisdiction");
            return null;
        }

        private static List<Violation> ClipViolationsToRanges(List<Violation> violations, List<JurisdictionRange> ranges)
        {
            var clipped = new List<Violation>();
            foreach (var violation in violations)
            {
                foreach (var range in ranges)
                {
                    var start = violation.StartTimestamp > range.Start ? violation.StartTimestamp : range.Start;
                    var end = violation.EndTimestamp < range.End ? violation.EndTimestamp : range.End;
                    if (end <= start)
                        continue;

                    if (start == violation.StartTimestamp && end == violation.EndTimestamp)
                    {
                        clipped.Add(violation);
                        continue;
                    }

                    var overLimitStart = violation.OverLimitStartTime > start ? violation.OverLimitStartTime : start;
                    var overLimitEnd = violation.OverLimitStartTime.Add(violation.OverLimitTotalSize);
                    if (overLimitEnd > end)
                        overLimitEnd = end;
                    var overLimitSize = overLimitEnd > overLimitStart ? overLimitEnd - overLimitStart : TimeSpan.Zero;

                    var size = end - start;
                    clipped.Add(new Violation(violation.DriverIdNumber, violation.TruckNumber,
                        overLimitStart, overLimitSize,
                        start, size,
                        violation.Limit, size, violation.Comment));
                }
            }

            return clipped;
        }

        private List<Violation> RunAudit(IRuleDefinition ruleDefinition, Moment startOfAuditWindow, Moment endOfAuditWindow, ITimelineNavigator navigator, IList<AuditRule> rulesToAudit, ILogger logger, List<DaySummary> daySummaries, CancellationToken cancellationToken = default)
        {
            var drivingDutyStatus = DutyStatuses.DrivingDutyStatus;
            var unbrokenDrivingRuleOptions = new UnbrokenRuleOptions(
                drivingDutyStatus,
                drivingDutyStatus,
                ruleDefinition.MaxUnbrokenDrivingLimit,
                ruleDefinition.AdverseConditionsLimitExtension,
                ruleDefinition.MinBreakSize,
                DutyStatuses.AllButDrivingDutyStatuses,
                $"Unbroken Driving {Violation.FormatHours(ruleDefinition.MaxUnbrokenDrivingLimit)} hour Limit", null, null,
                ThrowViolationsAt.DutyStatusChange);
            var drivingRuleOptions = new StandardRuleOptions(
                drivingDutyStatus,
                drivingDutyStatus,
                ruleDefinition.MinDrivingLimit,
                ruleDefinition.AdverseConditionsLimitExtension,
                $"Over {Violation.FormatHours(ruleDefinition.MinDrivingLimit)} hour Limit", null, null,
                ThrowViolationsAt.RestAccumulated);
            var shiftRuleOptions = new ShiftRuleOptions(
                DutyStatuses.AllNormalDutyStatuses,
                DutyStatuses.DrivingDutyStatus,
                DutyStatuses.WorkingDutyStatuses,
                ruleDefinition.MinShiftLimit,
                ruleDefinition.AdverseConditionsLimitExtension,
                ruleDefinition.ShiftExtensionSize,
                $"Over {Violation.FormatHours(ruleDefinition.MinShiftLimit)} hour Limit",
                () =>
                {
                    if (DutyStatuses.RestDutyStatuses.Contains(navigator.DutyStatus))
                        return !navigator.CurrentRestMoment.IsQualified;
                    return true;
                }, null,
                ThrowViolationsAt.RestAccumulated);
            var onDutyRuleOptions = new StandardRuleOptions(
                DutyStatuses.WorkingDutyStatuses,
                drivingDutyStatus,
                ruleDefinition.MinOnDutyLimit,
                ruleDefinition.AdverseConditionsLimitExtension,
                $"Over {Violation.FormatHours(ruleDefinition.MinOnDutyLimit)} hour Limit", null, null,
                ThrowViolationsAt.RestAccumulated);
            var windowRuleOptions = new WindowRuleOptions(
                    DutyStatuses.WorkingDutyStatuses,
                    DutyStatuses.DrivingDutyStatus,
                    ruleDefinition.NumberOfDaysInWindow,
                    ruleDefinition.MinWindowLimit,
                    TimeSpan.Zero,
                    $"Over {Violation.FormatHours(ruleDefinition.MinWindowLimit)} hour Limit", null, null,
                    //Auto-cycle rulesets keep the window rule for day summaries but report
                    //cycle violations through the CycleFeasibilityChecker instead.
                    ruleDefinition.WindowRuleThrowsViolations ? ThrowViolationsAt.EndOfDay : new List<ThrowViolations>());

            var unbrokenDrivingRule = new UnbrokenRule(navigator, unbrokenDrivingRuleOptions, logger);
            var drivingRule = new StandardRule(navigator, drivingRuleOptions, logger);
            var shiftRule = new ShiftRule(navigator, shiftRuleOptions, logger);
            var onDutyRule = new StandardRule(navigator, onDutyRuleOptions, logger);
            var windowRule = new WindowRule(navigator, new DailyRecap(navigator), windowRuleOptions, logger);
            var sleeperSplitRules = new RuleList();
            var dailyRules = new RuleList();
            if (rulesToAudit.Contains(AuditRule.UnbrokenDriving) && ruleDefinition.MaxUnbrokenDrivingLimit != TimeSpan.Zero)
                sleeperSplitRules.AddRule(unbrokenDrivingRule);
            if (rulesToAudit.Contains(AuditRule.Driving) && ruleDefinition.MinDrivingLimit != TimeSpan.Zero)
                sleeperSplitRules.AddRule(drivingRule);
            if (rulesToAudit.Contains(AuditRule.Shift) && ruleDefinition.MinShiftLimit != TimeSpan.Zero)
                sleeperSplitRules.AddRule(shiftRule);
            if (rulesToAudit.Contains(AuditRule.Shift) && ruleDefinition.MinOnDutyLimit != TimeSpan.Zero)
                sleeperSplitRules.AddRule(onDutyRule);
            if (rulesToAudit.Contains(AuditRule.Window))
                dailyRules.AddRule(windowRule);

            IRestTimelineBuilder restTimelineBuilder;
            IRestTimelinePairer restTimelinePairer;
            if (ruleDefinition.UsesPrimarySplit)
            {
                restTimelineBuilder = new RestTimelineBuilderUsaPrimary(logger, ruleDefinition, navigator);
                restTimelinePairer = new RestTimelinePairerUsaPrimary(logger, ruleDefinition, navigator);
            }
            else
            {
                restTimelineBuilder = new RestTimelineBuilderUsaBus(logger, ruleDefinition, navigator);
                restTimelinePairer = new RestTimelinePairerUsaBus(logger, ruleDefinition, navigator);
            }
           
            restTimelineBuilder.BuildTimeline(cancellationToken);
            restTimelinePairer.PairSleeperSplits(cancellationToken);

            var violationGateway = new ViolationGateway(logger);
            var sleeperSplitRuleLoop = new SleeperSplitRuleLoop(navigator, sleeperSplitRules, violationGateway, logger);
            sleeperSplitRuleLoop.MainLoop(startOfAuditWindow, endOfAuditWindow, cancellationToken);

            Func<DaySummary?>? snapshotFactory = rulesToAudit.Contains(AuditRule.Window)
                ? () => windowRule.CreateSnapshot()
                : null;
            var dailyRuleLoop = new DailyRuleLoop(navigator, dailyRules, violationGateway, logger, daySummaries, snapshotFactory);
            dailyRuleLoop.MainLoop(startOfAuditWindow, endOfAuditWindow, cancellationToken);

            var shiftExtAudit = new ShiftExtensionOveruseChecker.ShiftExtensionOveruseChecker(navigator, ruleDefinition, violationGateway, logger);
            shiftExtAudit.MainLoop(startOfAuditWindow, endOfAuditWindow, cancellationToken);

            if (ruleDefinition.MinDailyOffDuty > TimeSpan.Zero)
            {
                var dailyOffDutyChecker = new DailyOffDutyChecker(navigator, ruleDefinition, violationGateway, logger);
                dailyOffDutyChecker.MainLoop(startOfAuditWindow, endOfAuditWindow, cancellationToken);
            }

            if (ruleDefinition.MinExtendedRest > TimeSpan.Zero)
            {
                var extendedRestChecker = new ExtendedRestChecker(navigator, ruleDefinition, violationGateway, logger);
                extendedRestChecker.MainLoop(startOfAuditWindow, endOfAuditWindow, cancellationToken);
            }

            if (ruleDefinition.Cycle1WindowLimit > TimeSpan.Zero && ruleDefinition.Cycle2WindowLimit > TimeSpan.Zero)
            {
                var cycleFeasibilityChecker = new CycleFeasibilityChecker(navigator, ruleDefinition, violationGateway, logger);
                cycleFeasibilityChecker.MainLoop(startOfAuditWindow, endOfAuditWindow, cancellationToken);
            }

            AddProjectedRestMomentsForFinalRestSegment(navigator, endOfAuditWindow, ruleDefinition);

            return violationGateway.GetViolations();
        }

        private void AddProjectedRestMomentsForFinalRestSegment(ITimelineNavigator navigator, Moment endOfAuditWindow, IRuleDefinition ruleDefinition)
        {
            navigator.JumpTo(endOfAuditWindow.Timestamp);
            if (navigator.IsEndOfTime())
                navigator.Prior();
            if (navigator.IsBeginningOfTime() || !DutyStatuses.AllRestDutyStatuses.Contains(navigator.DutyStatus))
                return;

            var restStart = navigator.StartTimestamp;
            var driverIdNumber = navigator.DriverIdNumber;
            var truckNumber = navigator.TruckNumber;

            while (!navigator.IsBeginningOfTime())
            {
                navigator.Prior();
                if (!DutyStatuses.AllRestDutyStatuses.Contains(navigator.DutyStatus))
                    break;
                restStart = navigator.StartTimestamp;
                driverIdNumber = navigator.DriverIdNumber;
                truckNumber = navigator.TruckNumber;
            }

            var splitReachedAt = restStart.Add(ruleDefinition.MinSplitRest);
            navigator.Upsert(new RestMoment(splitReachedAt, splitReachedAt, TimeSpan.Zero, false, false, true, false, false, driverIdNumber, truckNumber));

            var primaryReachedAt = restStart.Add(ruleDefinition.MinPrimarySplitRest);
            navigator.Upsert(new RestMoment(primaryReachedAt, primaryReachedAt, TimeSpan.Zero, false, false, true, true, false, driverIdNumber, truckNumber));

            var fullRestReachedAt = restStart.Add(ruleDefinition.MinFullRest);
            navigator.Upsert(new RestMoment(fullRestReachedAt, fullRestReachedAt, TimeSpan.Zero, false, true, false, false, false, driverIdNumber, truckNumber));

            var globalResetReachedAt = restStart.Add(ruleDefinition.GlobalReset);
            navigator.Upsert(new RestMoment(globalResetReachedAt, globalResetReachedAt, TimeSpan.Zero, true, true, false, false, false, driverIdNumber, truckNumber));
        }

        private List<(DateTime Start, DateTime End, string? DriverIdNumber, string? TruckNumber)> GetSleeperSegmentsThrough(
            DateTime endTimestamp,
            ITimelineNavigator navigator)
        {
            var sleeperSegments = new List<(DateTime Start, DateTime End, string? DriverIdNumber, string? TruckNumber)>();
            DateTime? sleeperSegmentStart = null;
            string? sleeperDriverIdNumber = null;
            string? sleeperTruckNumber = null;

            navigator.JumpTo(DateTime.MinValue);
            while (!navigator.IsEndOfTime() && navigator.StartTimestamp < endTimestamp)
            {
                if (navigator.DutyStatus == DutyStatus.Sleeper)
                {
                    if (sleeperSegmentStart == null)
                    {
                        sleeperSegmentStart = navigator.StartTimestamp;
                        sleeperDriverIdNumber = navigator.DriverIdNumber;
                        sleeperTruckNumber = navigator.TruckNumber;
                    }
                }
                else if (sleeperSegmentStart != null)
                {
                    sleeperSegments.Add((sleeperSegmentStart.Value, navigator.StartTimestamp, sleeperDriverIdNumber, sleeperTruckNumber));
                    sleeperSegmentStart = null;
                    sleeperDriverIdNumber = null;
                    sleeperTruckNumber = null;
                }

                navigator.Next();
            }

            if (sleeperSegmentStart != null)
            {
                var segmentEnd = navigator.IsEndOfTime() ? endTimestamp : navigator.StartTimestamp;
                if (segmentEnd > endTimestamp)
                    segmentEnd = endTimestamp;

                if (segmentEnd > sleeperSegmentStart.Value)
                {
                    sleeperSegments.Add((sleeperSegmentStart.Value, segmentEnd, sleeperDriverIdNumber, sleeperTruckNumber));
                }
            }

            return sleeperSegments;
        }

        private List<(TimeSpan Size, bool IsPrimary)> BuildSplitPairingProjectionThresholds()
        {
            var minimumSizesForPairing = _ruleDefinition.UsesPrimarySplit
                ? new[] { (_ruleDefinition.MinPrimarySplitRest, false), (_ruleDefinition.MinSecondarySplitRest, true) }
                : new[] { (_ruleDefinition.MinSplitRest, false) };

            var uniqueThresholds = new Dictionary<TimeSpan, bool>();
            foreach (var minimumSize in minimumSizesForPairing)
            {
                var thresholdSize = _ruleDefinition.MinFullRest - minimumSize.Item1;
                if (thresholdSize <= TimeSpan.Zero)
                    continue;

                if (uniqueThresholds.TryGetValue(thresholdSize, out var existingIsPrimary))
                {
                    uniqueThresholds[thresholdSize] = existingIsPrimary || minimumSize.Item2;
                }
                else
                {
                    uniqueThresholds[thresholdSize] = minimumSize.Item2;
                }
            }

            return uniqueThresholds
                .Select(x => (x.Key, x.Value))
                .OrderBy(x => x.Key)
                .ToList();
        }

        private Moment DontAllowClearViolationsToEndAtEndOfTime(ITimelineNavigator navigator, Moment endOfAuditWindow)
        {
            if (endOfAuditWindow.Timestamp == DateTime.MaxValue)
            {
                navigator.JumpTo(DateTime.MaxValue);
                navigator.Prior();
                return navigator.IsBeginningOfTime() ? endOfAuditWindow : navigator.Start;
            }
            return endOfAuditWindow;
        }

        private RestTargets BuildRestTargets()
        {
            var sleeper = new SleeperRestTargets(
                _ruleDefinition.MinSplitRest,
                _ruleDefinition.MinPrimarySplitRest,
                _ruleDefinition.MinFullRest,
                _ruleDefinition.GlobalReset);
            var offDuty = new OffDutyRestTargets(
                _ruleDefinition.MinFullRest,
                _ruleDefinition.GlobalReset);
            return new RestTargets(sleeper, offDuty);
        }

        private Moment DontAllowClearViolationsToStartAtBeginningOfTime(ITimelineNavigator navigator, Moment startOfAuditWindow)
        {
            if (startOfAuditWindow.Timestamp == DateTime.MinValue)
            {
                navigator.JumpTo(DateTime.MinValue);
                navigator.Next();
                
                return navigator.IsEndOfTime() ? startOfAuditWindow : navigator.Start;
            }
            return startOfAuditWindow;
        }
    }
}
