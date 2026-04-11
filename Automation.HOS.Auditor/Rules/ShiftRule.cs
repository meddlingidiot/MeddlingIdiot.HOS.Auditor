using Automation.HOS.TimelineNavigator;
using Automation.HOS.TimelineNavigator.Utilities;

namespace Automation.HOS.Rules
{
    internal sealed class ShiftRule : RuleBase, IRule
    {
        private readonly ITimelineNavigator _navigator;
        private readonly ShiftRuleOptions _options;
        private readonly ILogger _logger;

        public ShiftRule(ITimelineNavigator navigator, ShiftRuleOptions options, ILogger logger) : base(navigator, options, logger)
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
                //if shift hasn't started yet, only accumulate if the duty status is one that starts the shift or
                //accumulate if duty status is one that accumulates violation time.
                if ((GetTotalSize() == TimeSpan.Zero) && 
                    (_options.DutyStatusesThatStartShiftTime.Count == 0) || (_options.DutyStatusesThatStartShiftTime.Contains(dutyStatus)) ||
                    (_options.DutyStatusesThatAccumulateViolationTime.Contains(dutyStatus)))
                {
                      DoSafeAccumulate(toAccumulate);

                }
                else if (GetTotalSize() > TimeSpan.Zero)
                {
                    DoSafeAccumulate(toAccumulate);
                }

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

            }
        }

        public override TimeSpan GetLimitSize()
        {
            if (_navigator.IsShiftExtended)
            {
                return _options.Limit.Add(_options.ShiftExtensionSize);
            }
            return base.GetLimitSize();
        }

        public override TimeSpan GetTotalSize()
        {
            return base.GetTotalSize();
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

            _logger.Debug(LoggerCategories.Rule, GetLimitSize() + " - Accumulate( Total: " + GetTotalSize() + " segment: " + _navigator.Length + ")");
            totalSize += toAccumulate;

        }
    }
}
