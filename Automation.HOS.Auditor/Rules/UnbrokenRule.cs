using Automation.HOS.TimelineNavigator;
using Automation.HOS.TimelineNavigator.Utilities;

namespace Automation.HOS.Rules
{
    internal sealed class UnbrokenRule : RuleBase, IRule
    {
        private readonly ITimelineNavigator _navigator;
        private readonly UnbrokenRuleOptions _options;
        private readonly ILogger _logger;

        internal TimeSpan totalBreakSize { get; set; }
        internal DateTime BreakStartTime { get; set; } = DateTime.MaxValue;


        public UnbrokenRule(ITimelineNavigator navigator, UnbrokenRuleOptions options, ILogger logger) : base(navigator, options, logger)
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
                ResetBreakTime();

                //if shift hasn't started yet, only accumulate if the duty status is one that starts the shift or
                //accumulate if duty status is one that accumulates violation time.
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
                //_logger.Debug(GetLimitSize() + " - not accumulatable. Total: " + TotalSize);
                if (_options.DutyStatusesThatAccumulateBreakTime.Contains(dutyStatus))
                {
                    totalBreakSize += toAccumulate;
                    SetBreakStartTimeIfNeeded();
                }

                if (totalBreakSize >= _options.MinBreakTime)
                {
                    _logger.Debug(LoggerCategories.Rule, _options.MinBreakTime + " - Total: " + totalBreakSize + " - Over Limit");
                    _logger.Debug(LoggerCategories.Rule, " - Total: " + GetTotalSize() + " - Break Found!");
                    Reset();
                }

            }
        }
        public override void GlobalReset()
        {
            base.GlobalReset();
        }

        public override void Reset()
        {
            base.Reset();
            ResetBreakTime();
        }

        public void ResetBreakTime()
        {
            BreakStartTime = DateTime.MaxValue;
            totalBreakSize = TimeSpan.Zero;
        }

        private void DoSafeAccumulate(TimeSpan toAccumulate)
        {
            if ((_options.OnShouldAccumulate != null) && (!_options.OnShouldAccumulate.Invoke())) return;

            _logger.Debug(LoggerCategories.Rule, GetLimitSize() + " - Accumulate( Total: " + GetTotalSize() + " segment: " + _navigator.Length + ")");
            totalSize += toAccumulate;

        }

        private void SetBreakStartTimeIfNeeded()
        {
            if (BreakStartTime == DateTime.MaxValue)
            {
                BreakStartTime = _navigator.StartTimestamp;
            }
        }
    }
}
