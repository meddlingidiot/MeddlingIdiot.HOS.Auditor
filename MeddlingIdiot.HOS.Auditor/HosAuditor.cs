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
  
            var drivingDutyStatus = DutyStatuses.DrivingDutyStatus;
            var unbrokenDrivingRuleOptions = new UnbrokenRuleOptions(
                drivingDutyStatus,
                drivingDutyStatus,
                _ruleDefinition.MaxUnbrokenDrivingLimit,
                _ruleDefinition.AdverseConditionsLimitExtension,
                _ruleDefinition.MinBreakSize,
                DutyStatuses.AllButDrivingDutyStatuses,
                $"Unbroken Driving {Violation.FormatHours(_ruleDefinition.MaxUnbrokenDrivingLimit)} hour Limit", null, null, 
                ThrowViolationsAt.DutyStatusChange);
            var drivingRuleOptions = new StandardRuleOptions(
                drivingDutyStatus, 
                drivingDutyStatus, 
                _ruleDefinition.MinDrivingLimit,
                _ruleDefinition.AdverseConditionsLimitExtension,
                $"Over {Violation.FormatHours(_ruleDefinition.MinDrivingLimit)} hour Limit", null, null,
                ThrowViolationsAt.RestAccumulated);
            var shiftRuleOptions = new ShiftRuleOptions(
                DutyStatuses.AllNormalDutyStatuses, 
                DutyStatuses.DrivingDutyStatus, 
                DutyStatuses.WorkingDutyStatuses, 
                _ruleDefinition.MinShiftLimit,
                _ruleDefinition.AdverseConditionsLimitExtension,
                _ruleDefinition.ShiftExtensionSize,
                $"Over {Violation.FormatHours(_ruleDefinition.MinShiftLimit)} hour Limit",
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
                _ruleDefinition.MinOnDutyLimit,
                _ruleDefinition.AdverseConditionsLimitExtension,
                $"Over {Violation.FormatHours(_ruleDefinition.MinOnDutyLimit)} hour Limit", null, null,
                ThrowViolationsAt.RestAccumulated);
            var windowRuleOptions = new WindowRuleOptions(
                    DutyStatuses.WorkingDutyStatuses, 
                    DutyStatuses.DrivingDutyStatus, 
                    _ruleDefinition.NumberOfDaysInWindow,
                    _ruleDefinition.MinWindowLimit,
                    TimeSpan.Zero,
                    $"Over {Violation.FormatHours(_ruleDefinition.MinWindowLimit)} hour Limit", null, null,
                    ThrowViolationsAt.EndOfDay);

            var unbrokenDrivingRule = new UnbrokenRule(navigator, unbrokenDrivingRuleOptions, logger);
            var drivingRule = new StandardRule(navigator, drivingRuleOptions, logger);
            var shiftRule = new ShiftRule(navigator, shiftRuleOptions, logger);
            var onDutyRule = new StandardRule(navigator, onDutyRuleOptions, logger);
            var windowRule = new WindowRule(navigator, new DailyRecap(navigator), windowRuleOptions, logger);
            var sleeperSplitRules = new RuleList();
            var dailyRules = new RuleList();
            if (rulesToAudit.Contains(AuditRule.UnbrokenDriving) && _ruleDefinition.MaxUnbrokenDrivingLimit != TimeSpan.Zero)
                sleeperSplitRules.AddRule(unbrokenDrivingRule);
            if (rulesToAudit.Contains(AuditRule.Driving) && _ruleDefinition.MinDrivingLimit != TimeSpan.Zero)
                sleeperSplitRules.AddRule(drivingRule);
            if (rulesToAudit.Contains(AuditRule.Shift) && _ruleDefinition.MinShiftLimit != TimeSpan.Zero)
                sleeperSplitRules.AddRule(shiftRule);
            if (rulesToAudit.Contains(AuditRule.Shift) && _ruleDefinition.MinOnDutyLimit != TimeSpan.Zero)
                sleeperSplitRules.AddRule(onDutyRule);
            if (rulesToAudit.Contains(AuditRule.Window))
                dailyRules.AddRule(windowRule);

            IRestTimelineBuilder restTimelineBuilder;
            IRestTimelinePairer restTimelinePairer;
            if (_ruleDefinition.UsesPrimarySplit)
            {
                restTimelineBuilder = new RestTimelineBuilderUsaPrimary(logger, _ruleDefinition, navigator);
                restTimelinePairer = new RestTimelinePairerUsaPrimary(logger, _ruleDefinition, navigator);
            }
            else
            {
                restTimelineBuilder = new RestTimelineBuilderUsaBus(logger, _ruleDefinition, navigator);
                restTimelinePairer = new RestTimelinePairerUsaBus(logger, _ruleDefinition, navigator);
            }
           
            restTimelineBuilder.BuildTimeline(cancellationToken);
            restTimelinePairer.PairSleeperSplits(cancellationToken);

            var daySummaries = new List<DaySummary>();
            var violationGateway = new ViolationGateway(logger);
            var sleeperSplitRuleLoop = new SleeperSplitRuleLoop(navigator, sleeperSplitRules, violationGateway, logger);
            sleeperSplitRuleLoop.MainLoop(startOfAuditWindow, endOfAuditWindow, cancellationToken);

            Func<DaySummary?>? snapshotFactory = rulesToAudit.Contains(AuditRule.Window)
                ? () => windowRule.CreateSnapshot()
                : null;
            var dailyRuleLoop = new DailyRuleLoop(navigator, dailyRules, violationGateway, logger, daySummaries, snapshotFactory);
            dailyRuleLoop.MainLoop(startOfAuditWindow, endOfAuditWindow, cancellationToken);

            var shiftExtAudit = new ShiftExtensionOveruseChecker.ShiftExtensionOveruseChecker(navigator, _ruleDefinition, violationGateway, logger);
            shiftExtAudit.MainLoop(startOfAuditWindow, endOfAuditWindow, cancellationToken);

            AddProjectedRestMomentsForFinalRestSegment(navigator, endOfAuditWindow);

            var violations = violationGateway.GetViolations();
            startOfAuditWindow = DontAllowClearViolationsToStartAtBeginningOfTime(navigator, startOfAuditWindow);
            endOfAuditWindow = DontAllowClearViolationsToEndAtEndOfTime(navigator, endOfAuditWindow);
            var clearViolationRange =
                new ClearViolationRange(startOfAuditWindow.Timestamp, endOfAuditWindow.Timestamp);

            if (cancellationToken.IsCancellationRequested)
                return new ViolationResults([], clearViolationRange, logger.GetResults());
            
            var violationResults = new ViolationResults(violations, clearViolationRange, logger.GetResults(), daySummaries, navigator.GetRestTimelineMoments());

            return violationResults;
        }

        private void AddProjectedRestMomentsForFinalRestSegment(ITimelineNavigator navigator, Moment endOfAuditWindow)
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

            var splitReachedAt = restStart.Add(_ruleDefinition.MinSplitRest);
            navigator.Upsert(new RestMoment(splitReachedAt, splitReachedAt, TimeSpan.Zero, false, false, true, false, false, driverIdNumber, truckNumber));

            var primaryReachedAt = restStart.Add(_ruleDefinition.MinPrimarySplitRest);
            navigator.Upsert(new RestMoment(primaryReachedAt, primaryReachedAt, TimeSpan.Zero, false, false, true, true, false, driverIdNumber, truckNumber));

            var fullRestReachedAt = restStart.Add(_ruleDefinition.MinFullRest);
            navigator.Upsert(new RestMoment(fullRestReachedAt, fullRestReachedAt, TimeSpan.Zero, false, true, false, false, false, driverIdNumber, truckNumber));

            var globalResetReachedAt = restStart.Add(_ruleDefinition.GlobalReset);
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
