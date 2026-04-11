using Automation.HOS.TimelineNavigator;

namespace Automation.HOS.Rules
{
    internal class RuleOptionsBase
    {
        public List<DutyStatus> DutyStatusesThatAccumulateTime { get; private set; } = new List<DutyStatus>();
        public List<DutyStatus> DutyStatusesThatAccumulateViolationTime { get; private set; } = new List<DutyStatus>();
        public TimeSpan Limit { get; private set; }
        public TimeSpan AdverseConditionsLimitExtension { get; private set; }
        public string Comment { get; private set; }
        public Func<bool>? OnShouldAccumulate { get; private set; }
        public Action<DateTime, string?, string?>? OnLimitReached { get; private set; }
        public List<ThrowViolations> ThrowViolations { get; private set; } = new List<ThrowViolations>();

        public RuleOptionsBase(
            IEnumerable<DutyStatus> dutyStatusesThatAccumulateTime,
            IEnumerable<DutyStatus> dutyStatusesThatAccumulateViolationTime,
            TimeSpan limit,
            TimeSpan adverseConditionsLimitExtension,
            string comment,
            Func<bool>? onShouldAccumulate,
            Action<DateTime, string?, string?>? onLimitReached,
            IEnumerable<ThrowViolations> throwViolations)
        {
            DutyStatusesThatAccumulateTime.AddRange(dutyStatusesThatAccumulateTime);
            DutyStatusesThatAccumulateViolationTime.AddRange(dutyStatusesThatAccumulateViolationTime);
            Limit = limit;
            AdverseConditionsLimitExtension = adverseConditionsLimitExtension;
            OnShouldAccumulate = onShouldAccumulate;
            OnLimitReached = onLimitReached;
            Comment = comment;
            ThrowViolations.AddRange(throwViolations);
        }
    }

    internal class StandardRuleOptions : RuleOptionsBase
    {
        public StandardRuleOptions(
            IEnumerable<DutyStatus> dutyStatusesThatAccumulateTime,
            IEnumerable<DutyStatus> dutyStatusesThatAccumulateViolationTime,
            TimeSpan limit,
            TimeSpan adverseConditionsLimitExtension,
            string comment,
            Func<bool>? onShouldAccumulate,
            Action<DateTime, string?, string?>? onLimitReached,
            IEnumerable<ThrowViolations> throwViolations) : base(dutyStatusesThatAccumulateTime,
            dutyStatusesThatAccumulateViolationTime, limit, 
            adverseConditionsLimitExtension, comment, onShouldAccumulate, onLimitReached,
            throwViolations)
        {
        }
    }

    internal class ShiftRuleOptions : RuleOptionsBase
    {
        public List<DutyStatus> DutyStatusesThatStartShiftTime { get; private set; } = new List<DutyStatus>();
        public TimeSpan ShiftExtensionSize { get; private set; }

        public ShiftRuleOptions(
                IEnumerable<DutyStatus> dutyStatusesThatAccumulateTime,
                IEnumerable<DutyStatus> dutyStatusesThatAccumulateViolationTime,
                IEnumerable<DutyStatus> dutyStatusesThatStartShiftTime,
                TimeSpan limit,
                TimeSpan adverseConditionsLimitExtension,
                TimeSpan shiftExtensionSize,
                string comment,
                Func<bool>? onShouldAccumulate,
                Action<DateTime, string?, string?>? onLimitReached, 
                IEnumerable<ThrowViolations> throwViolations) : base(dutyStatusesThatAccumulateTime, 
            dutyStatusesThatAccumulateViolationTime, limit, 
            adverseConditionsLimitExtension,
            comment, onShouldAccumulate, onLimitReached, throwViolations)
        {
             DutyStatusesThatStartShiftTime.AddRange(dutyStatusesThatStartShiftTime); 
             ShiftExtensionSize = shiftExtensionSize;
        }

    }

    internal class WindowRuleOptions : RuleOptionsBase
    {
        public int DaysInWindow { get; private set; }

        public WindowRuleOptions(
            IEnumerable<DutyStatus> dutyStatusesThatAccumulateTime,
            IEnumerable<DutyStatus> dutyStatusesThatAccumulateViolationTime,
            int daysInWindow,
            TimeSpan limit,
            TimeSpan adverseConditionsLimitExtension,
            string comment,
            Func<bool>? onShouldAccumulate,
            Action<DateTime, string?, string?>? onLimitReached,
            IEnumerable<ThrowViolations> throwViolations) : base(dutyStatusesThatAccumulateTime,
            dutyStatusesThatAccumulateViolationTime, limit, 
            adverseConditionsLimitExtension, 
            comment, onShouldAccumulate, onLimitReached, throwViolations)
        {
            DaysInWindow = daysInWindow;
        }
    }

    internal class UnbrokenRuleOptions : RuleOptionsBase
    {
        public TimeSpan MinBreakTime { get; private set; }
        public List<DutyStatus> DutyStatusesThatAccumulateBreakTime { get; private set; } = new List<DutyStatus>();
        
        public UnbrokenRuleOptions(
            IEnumerable<DutyStatus> dutyStatusesThatAccumulateTime,
            IEnumerable<DutyStatus> dutyStatusesThatAccumulateViolationTime,
            TimeSpan limit,
            TimeSpan adverseConditionsLimitExtension,
            TimeSpan minBreakTime,
            IEnumerable<DutyStatus> dutyStatusesThatCountAsBreakTime,
            string comment,
            Func<bool>? onShouldAccumulate,
            Action<DateTime, string?, string?>? onLimitReached,
            IEnumerable<ThrowViolations> throwViolations) : base(dutyStatusesThatAccumulateTime,
            dutyStatusesThatAccumulateViolationTime, limit, adverseConditionsLimitExtension,
            comment, onShouldAccumulate, onLimitReached, throwViolations)
        {
            MinBreakTime = minBreakTime;
            DutyStatusesThatAccumulateBreakTime.AddRange(dutyStatusesThatCountAsBreakTime);
        }
    }
}
