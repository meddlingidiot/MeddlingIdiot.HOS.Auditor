using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Utilities;
using MeddlingIdiot.HOS.Violations;

namespace MeddlingIdiot.HOS.Rules
{
    internal abstract class RuleBase
    {
        private readonly ITimelineNavigator _navigator;
        private readonly RuleOptionsBase _options;
        private readonly ILogger _logger;

        public DateTime OverLimitStartTime { get; internal set; } = DateTime.MaxValue;
        public DateTime OverLimitFinishTime { get; internal set; } = DateTime.MaxValue;
        internal TimeSpan totalSize { get; set; }
        internal TimeSpan LimitExtensionSize { get; set; } = TimeSpan.Zero;
        public DateTime InViolationStartTime { get; internal set; } = DateTime.MaxValue;
        public TimeSpan InViolationTotalSize { get; internal set; } = TimeSpan.Zero;
        public DateTime InViolationFinishTime { get; internal set; } = DateTime.MaxValue;
        public TimeSpan TimeInViolation { get; internal set; } = TimeSpan.Zero;
        public string? DriverIdNumberOfStart { get; internal set; }
        public string? TruckNumberOfStart { get; internal set; }
        public bool HasViolation => InViolationTotalSize > TimeSpan.Zero;
        public virtual TimeSpan GetTotalSize() => totalSize;
        public virtual TimeSpan GetLimitSize()
        {
            return _options.Limit.Add(LimitExtensionSize);
        }

        public IEnumerable<ThrowViolations> ThrowViolations => _options.ThrowViolations;

        protected RuleBase(ITimelineNavigator navigator, RuleOptionsBase options, ILogger logger)
        {
            _navigator = navigator;
            _options = options;
            _logger = logger;
        }

        protected TimeSpan OverLimitThisSegment
        {
            get
            {
                var hoursOverLimit = GetTotalSize() - GetLimitSize();
                return hoursOverLimit > TimeSpan.Zero ? hoursOverLimit : TimeSpan.Zero;
            }
        }

        protected TimeSpan GetTimeInViolationThisSegment(TimeSpan maxSegmentLength)
        {
                var timeInViolation = GetTotalSize() - GetLimitSize();
                var timeInViolationOrZero = timeInViolation > TimeSpan.Zero ? timeInViolation : TimeSpan.Zero;
                if (timeInViolationOrZero > maxSegmentLength)
                {
                    timeInViolationOrZero = maxSegmentLength;
                }

                return timeInViolationOrZero;
        }

        internal void SetInViolationFinishIfNeeded(DutyStatus dutyStatus, TimeSpan toAccumulate)
        {
            if ((InViolationStartTime != DateTime.MaxValue) &&
                (_options.DutyStatusesThatAccumulateViolationTime.Contains(dutyStatus)))
            {
                InViolationFinishTime = _navigator.FinishTimestamp;
                TimeInViolation += GetTimeInViolationThisSegment(toAccumulate);
            }
        }

        internal void SetInViolationIfNeeded(DutyStatus dutyStatus, TimeSpan toAccumulate)
        {
            if ((_options.DutyStatusesThatAccumulateViolationTime.Contains(dutyStatus)) &&
                (GetTotalSize() > GetLimitSize()) && (InViolationStartTime == DateTime.MaxValue))
            {
                if (OverLimitStartTime == DateTime.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(OverLimitStartTime));

                InViolationStartTime = _navigator.FinishTimestamp.Add(-GetTimeInViolationThisSegment(toAccumulate));
                InViolationTotalSize += GetTimeInViolationThisSegment(toAccumulate);
            }
        }

        internal void SetOverLimitFinishTimeIfNeeded(DutyStatus dutyStatus)
        {
            if ((GetTotalSize() > GetLimitSize()) && (_options.DutyStatusesThatAccumulateTime.Contains(dutyStatus)))
            {
                OverLimitFinishTime = _navigator.FinishTimestamp;
            }
        }

        internal void SetOverLimitStartTimeIfNeeded()
        {
            if ((GetTotalSize() > GetLimitSize()) && (OverLimitStartTime == DateTime.MaxValue))
            {
                OverLimitStartTime = _navigator.FinishTimestamp.Add(-OverLimitThisSegment);
                DriverIdNumberOfStart = _navigator.DriverIdNumber;
                TruckNumberOfStart = _navigator.TruckNumber;
            }
        }

        public virtual bool ShouldAccumulate(DateTime startTimestamp, TimeSpan toAccumulate, DutyStatus dutyStatus)
        {
            if (_navigator.IsAgriculturalExceptionEnabled)
            {
                _logger.Debug(LoggerCategories.Rule, $"|{_options.Limit.Add(LimitExtensionSize)}| Agricultural Exception Enabled at :{startTimestamp}");
                return false;
            }

            if (_navigator.IsAdverseConditionsEnabled)
            {
                LimitExtensionSize = _options.AdverseConditionsLimitExtension;
                _logger.Debug(LoggerCategories.Rule, $"|{_options.Limit.Add(LimitExtensionSize)}| Adverse Conditions Enabled at : {startTimestamp}");
            }


            return _options.DutyStatusesThatAccumulateTime.Contains(dutyStatus);
        }

        public abstract void Accumulate(DateTime startTimestamp, TimeSpan toAccumulate, DutyStatus dutyStatus);

        public virtual void GlobalReset()
        {
            Reset();
        }

        public virtual void Reset()
        {
            _logger.Debug(LoggerCategories.Rule, GetLimitSize() + " - Reset");
            totalSize = TimeSpan.Zero;
            OverLimitStartTime = DateTime.MaxValue;
            OverLimitFinishTime = DateTime.MaxValue;
            InViolationStartTime = DateTime.MaxValue;
            InViolationTotalSize = TimeSpan.Zero;
            InViolationFinishTime = DateTime.MaxValue;
            TimeInViolation = TimeSpan.Zero;
            DriverIdNumberOfStart = null;
            TruckNumberOfStart = null;
            LimitExtensionSize = TimeSpan.Zero;
        }

        public virtual Violation? GetViolation()
        {
            if (HasViolation)
            {
                //Special note: we use _options.Limit here because the extension is not "rule" change.
                return new Violation(DriverIdNumberOfStart, TruckNumberOfStart, OverLimitStartTime,
                    OverLimitFinishTime - OverLimitStartTime,
                    InViolationStartTime, 
                    InViolationFinishTime - InViolationStartTime,
                    _options.Limit, TimeInViolation, _options.Comment);
            }

            return null;
        }
    }
}
