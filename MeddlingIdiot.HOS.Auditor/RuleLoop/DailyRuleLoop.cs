using MeddlingIdiot.HOS.Rules;
using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;
using MeddlingIdiot.HOS.TimelineNavigator.Utilities;
using MeddlingIdiot.HOS.Violations;

namespace MeddlingIdiot.HOS.RuleLoop
{
    internal class DailyRuleLoop : RuleLoop
    {
        private readonly ITimelineNavigator _navigator;
        private readonly IRuleList _rules;
        private readonly IViolationGateway _violationGateway;
        private readonly ILogger _logger;

        public DailyRuleLoop(ITimelineNavigator navigator, IRuleList ruleList, IViolationGateway violationGateway, ILogger logger)
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
            _logger.Debug(LoggerCategories.DailyLoop, "------------------------------------");
            _logger.Debug(LoggerCategories.DailyLoop, "Start of audit window: " + startOfAuditWindow.Timestamp);
            _navigator.JumpTo(startOfAuditWindow.Timestamp);
            do
            {
                if (_navigator.CurrentRestMoment.IsGlobalReset)
                {
                    _logger.Debug(LoggerCategories.DailyLoop, "   global reset.");
                    ThrowViolations(Rules.ThrowViolations.AtRestAccumulated);
                    GlobalReset();
                }

                if (_navigator.IsStartOfDay)
                {
                    _logger.Debug(LoggerCategories.DailyLoop, "    new day.");
                    ThrowViolations(Rules.ThrowViolations.AtEndOfDay);
                    Reset();
                }

                _logger.Debug(LoggerCategories.DailyLoop, _navigator.CurrentDutyStatusChangeMoment.ToString());
                Accumulate(_navigator.Length, _navigator.DutyStatus);
 
                _navigator.Next();


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
                _logger.Debug(LoggerCategories.DailyLoop, "Violation: " + violation.ToString());
                _violationGateway.SaveViolation(violation);
            }
        }

    }


}
