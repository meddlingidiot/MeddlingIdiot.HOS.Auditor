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
                $"Unbroken Driving {_ruleDefinition.MaxUnbrokenDrivingLimit} hour Limit", null, null, 
                ThrowViolationsAt.DutyStatusChange);
            var drivingRuleOptions = new StandardRuleOptions(
                drivingDutyStatus, 
                drivingDutyStatus, 
                _ruleDefinition.MinDrivingLimit,
                _ruleDefinition.AdverseConditionsLimitExtension,
                $"Over {_ruleDefinition.MinDrivingLimit} hour Limit", null, null,
                ThrowViolationsAt.RestAccumulated);
            var shiftRuleOptions = new ShiftRuleOptions(
                DutyStatuses.AllNormalDutyStatuses, 
                DutyStatuses.DrivingDutyStatus, 
                DutyStatuses.WorkingDutyStatuses, 
                _ruleDefinition.MinShiftLimit,
                _ruleDefinition.AdverseConditionsLimitExtension,
                _ruleDefinition.ShiftExtensionSize,
                $"Over {_ruleDefinition.MinShiftLimit} hour Limit",
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
                $"Over {_ruleDefinition.MinOnDutyLimit} hour Limit", null, null,
                ThrowViolationsAt.RestAccumulated);
            var windowRuleOptions = new WindowRuleOptions(
                    DutyStatuses.WorkingDutyStatuses, 
                    DutyStatuses.DrivingDutyStatus, 
                    _ruleDefinition.NumberOfDaysInWindow,
                    _ruleDefinition.MinWindowLimit,
                    TimeSpan.Zero,
                    $"Over {_ruleDefinition.MinWindowLimit} hour Limit", null, null,
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

            var violationGateway = new ViolationGateway(logger);
            var sleeperSplitRuleLoop = new SleeperSplitRuleLoop(navigator, sleeperSplitRules, violationGateway, logger);
            sleeperSplitRuleLoop.MainLoop(startOfAuditWindow, endOfAuditWindow, cancellationToken);

            var dailyRuleLoop = new DailyRuleLoop(navigator, dailyRules, violationGateway, logger);
            dailyRuleLoop.MainLoop(startOfAuditWindow, endOfAuditWindow, cancellationToken);

            var shiftExtAudit = new ShiftExtensionOveruseChecker.ShiftExtensionOveruseChecker(navigator, _ruleDefinition, violationGateway, logger);
            shiftExtAudit.MainLoop(startOfAuditWindow, endOfAuditWindow, cancellationToken);

            var violations = violationGateway.GetViolations();
            startOfAuditWindow = DontAllowClearViolationsToStartAtBeginningOfTime(navigator, startOfAuditWindow);
            endOfAuditWindow = DontAllowClearViolationsToEndAtEndOfTime(navigator, endOfAuditWindow);
            var clearViolationRange =
                new ClearViolationRange(startOfAuditWindow.Timestamp, endOfAuditWindow.Timestamp);

            if (cancellationToken.IsCancellationRequested)
                return new ViolationResults([], clearViolationRange, logger.GetResults());
            
            var violationResults = new ViolationResults(violations, clearViolationRange, logger.GetResults());

            return violationResults;
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
