using MeddlingIdiot.HOS.Rules;
using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;
using MeddlingIdiot.HOS.TimelineNavigator.Utilities;
using MeddlingIdiot.HOS.Violations;

namespace MeddlingIdiot.HOS.RuleLoop
{
    internal class SleeperSplitRuleLoop : RuleLoop
    {
        private readonly ITimelineNavigator _navigator;
        private readonly IRuleList _rules;
        private readonly IViolationGateway _violationGateway;
        private readonly ILogger _logger;

        private DutyStatus currentDutyStatus = DutyStatus.Unknown;

        public SleeperSplitRuleLoop(ITimelineNavigator navigator, IRuleList ruleList, IViolationGateway violationGateway, ILogger logger)
        {
            _navigator = navigator;
            _rules = ruleList;
            _violationGateway = violationGateway;
            _logger = logger;
        }

        public override void Accumulate(TimeSpan toAccumulate, DutyStatus dutyStatus)
        {
            _rules.Accumulate(_navigator.StartTimestamp, _navigator.Length, _navigator.DutyStatus);
        }

        public override void MainLoop(Moment startOfAuditWindow, Moment endOfAuditWindow)
        {
            _logger.Debug(LoggerCategories.SleeperSplitLoop, "------------------------------------");
            _logger.Debug(LoggerCategories.SleeperSplitLoop, "Start of audit window: " + startOfAuditWindow.Timestamp);

            var furthestSeen = DateTime.MinValue;
            var pairedSplitCount = 0;
            _navigator.JumpTo(startOfAuditWindow.Timestamp);
            do
            {
                if (currentDutyStatus != _navigator.DutyStatus)
                {
                    _logger.Debug(LoggerCategories.SleeperSplitLoop, "Duty status change: " + _navigator.DutyStatus);
                    currentDutyStatus = _navigator.DutyStatus;
                    ThrowViolations(Rules.ThrowViolations.AtDutyStatusChange);
                }

                _logger.Debug(LoggerCategories.SleeperSplitLoop, _navigator.CurrentDutyStatusChangeMoment.ToString());
                Accumulate(_navigator.Length, _navigator.DutyStatus);
                if (_navigator.CurrentRestMoment.IsPaired)
                {
                    pairedSplitCount++;
                    _logger.Debug(LoggerCategories.SleeperSplitLoop, "Paired split rest added " + _navigator.CurrentRestMoment.ToString());
                }

                if ((_navigator.CurrentRestMoment.IsPaired) && (pairedSplitCount >= 2) &&
                    (furthestSeen < _navigator.StartTimestamp))
                {
                    ThrowViolations(Rules.ThrowViolations.AtRestAccumulated);

                    furthestSeen = _navigator.StartTimestamp;
                    _navigator.JumpToPriorRest(true);
                    _logger.Debug(LoggerCategories.SleeperSplitLoop, $"   jump to prior rest ({_navigator.StartTimestamp}. (Reset)");
                    Reset();
                }
                else if (_navigator.CurrentRestMoment.IsFullRest)
                {
                    ThrowViolations(Rules.ThrowViolations.AtRestAccumulated);
                    _logger.Debug(LoggerCategories.SleeperSplitLoop, "   (FullRest)");
                    Reset();
                    _navigator.Next();
                }
                else if (_navigator.CurrentRestMoment.IsGlobalReset)
                {
                    ThrowViolations(Rules.ThrowViolations.AtRestAccumulated);
                    _logger.Debug(LoggerCategories.SleeperSplitLoop, "   (GlobalReset)");
                    GlobalReset();
                    _navigator.Next();
                }
                else
                {
                    _navigator.Next();
                }


            } while (_navigator.Finish.Timestamp <= endOfAuditWindow.Timestamp);

            ThrowViolations(Rules.ThrowViolations.AtEndOfAuditWindow);

        }

        public override void GlobalReset()
        {
            _rules.GlobalReset();
        }

        public override void Reset()
        {
            _rules.Reset();
        }

        public override void ThrowViolations(ThrowViolations firedAt)
        {
            var violations = _rules.GetViolations(firedAt);
            foreach (var violation in violations)
            {
                _logger.Debug(LoggerCategories.SleeperSplitLoop, "Violation: " + violation.ToString());
                _violationGateway.SaveViolation(violation);
            }
        }
    }
}
