using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Utilities;
using MeddlingIdiot.HOS.Violations;

namespace MeddlingIdiot.HOS.Rules
{

    internal sealed class WindowRule : RuleBase, IRule
    {
        private readonly ITimelineNavigator _navigator;
        private readonly DailyRecap _dailyRecap;
        private readonly WindowRuleOptions _options;
        private readonly ILogger _logger;
        
        private DateTime AuditDay { get; set; } = DateTime.MaxValue;
        private TimeSpan UsedBeforeToday { get; set; } = TimeSpan.Zero;
        private TimeSpan UsedToday { get; set; } = TimeSpan.Zero;

        private TimeSpan TotalUsed => UsedBeforeToday + UsedToday;
        
        public override TimeSpan GetTotalSize() => TotalUsed;

        public WindowRule(ITimelineNavigator navigator, DailyRecap dailyRecap, WindowRuleOptions ruleOptions, ILogger logger) : base(navigator, ruleOptions, logger)
        {
            _navigator = navigator;
            _dailyRecap = dailyRecap;
            _options = ruleOptions;
            _logger = logger;

            Reset();
        }

        public override bool ShouldAccumulate(DateTime startTimestamp, TimeSpan toAccumulate, DutyStatus dutyStatus)
        {
            //WindowRule doesn't care about AgException.
            return _options.DutyStatusesThatAccumulateTime.Contains(dutyStatus);
        }

        public override void Accumulate(DateTime startTimestamp, TimeSpan toAccumulate, DutyStatus dutyStatus)
        {
            if (startTimestamp == DateTime.MinValue)
                return; //Don't accumulate if the start time is BOT.

            //Populate the DailyRecap
            if (ShouldAccumulate(startTimestamp, toAccumulate, dutyStatus))
            {
                _dailyRecap.Accumulate(startTimestamp, toAccumulate, dutyStatus);
                
                UsedToday += toAccumulate;

                SetOverLimitStartTimeIfNeeded();
                SetOverLimitFinishTimeIfNeeded(dutyStatus);

                SetInViolationIfNeeded(dutyStatus, toAccumulate);

                SetInViolationFinishIfNeeded(dutyStatus, toAccumulate);

                if (GetTotalSize() > GetLimitSize())
                {
                    _logger.Debug(LoggerCategories.Rule, GetLimitSize() + " - Total: " + GetTotalSize() + " - Limit Reached!");
                    _options.OnLimitReached?.Invoke(OverLimitStartTime, DriverIdNumberOfStart, TruckNumberOfStart);
                }
            }
        }

        public override void GlobalReset()
        {
            base.GlobalReset();
            _dailyRecap.Reset();
        }

        public override void Reset() // Could be called NewDay()
        {
            base.Reset();
            AuditDay = _navigator.CurrentDay;
            UsedBeforeToday = _dailyRecap.GetTotalUsed(AuditDay, _options.DaysInWindow);
            UsedToday = TimeSpan.Zero;
        }

        public override Violation? GetViolation()
        {
            var violation = base.GetViolation();
            if (violation == null) 
                _logger.Debug(LoggerCategories.Rule, "no Violation");
            else
                _logger.Debug(LoggerCategories.Rule, "Violation: " + violation.ToString());
            return violation;

        }
    } 

}