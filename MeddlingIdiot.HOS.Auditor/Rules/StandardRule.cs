using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Utilities;

namespace MeddlingIdiot.HOS.Rules
{
    internal sealed class StandardRule : RuleBase, IRule
    {
        private readonly ITimelineNavigator _navigator;
        private readonly StandardRuleOptions _options;
        private readonly ILogger _logger;

        public StandardRule(ITimelineNavigator navigator, StandardRuleOptions options, ILogger logger) : base(navigator, options, logger)
        {
            _navigator = navigator;
            _options = options;
            _logger = logger;
            
            Reset();
        }

        public override void Accumulate(DateTime startTimestamp, TimeSpan toAccumulate, DutyStatus dutyStatus)
        {
            if (startTimestamp == DateTime.MinValue)
                //Don't accumulate if the start time is BOT.
                return;

            if (ShouldAccumulate(startTimestamp, toAccumulate, dutyStatus))
            {
                DoSafeAccumulate(toAccumulate); 
                
                SetOverLimitStartTimeIfNeeded();
                SetOverLimitFinishTimeIfNeeded(dutyStatus);

                SetInViolationIfNeeded(dutyStatus, toAccumulate);

                SetInViolationFinishIfNeeded(dutyStatus, toAccumulate);

                if (GetTotalSize() >= GetLimitSize())
                {
                    _logger.Debug(LoggerCategories.Rule, GetLimitSize() + " - Total: " + GetTotalSize() + " - Limit Reached!");
                    _options.OnLimitReached?.Invoke(OverLimitStartTime, DriverIdNumberOfStart, TruckNumberOfStart);
                }
            }
            else
            {

            }
        }

        public override void GlobalReset()
        {
            base.GlobalReset();
        }

        public override void Reset()
        {
            base.Reset();
        }

        private void DoSafeAccumulate(TimeSpan toAccumulate)
        {
            if ((_options.OnShouldAccumulate != null) && (!_options.OnShouldAccumulate.Invoke())) return;

            totalSize += toAccumulate;
            _logger.Debug(LoggerCategories.Rule, GetLimitSize() + " - Accumulate( Total: " + GetTotalSize() + " segment: " + _navigator.Length + ")");
        }
    }
}
