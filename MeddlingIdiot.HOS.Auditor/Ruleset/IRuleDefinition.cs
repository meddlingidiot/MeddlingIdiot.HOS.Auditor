using MeddlingIdiot.HOS.TimelineNavigator;

namespace MeddlingIdiot.HOS.Ruleset
{
    public interface IRuleDefinition
    {
        public TimeSpan GlobalReset { get; }
        public TimeSpan MinFullRest { get; }
        public bool UsesPrimarySplit { get; }
        public TimeSpan MinPrimarySplitRest { get; } 
        public TimeSpan MinSecondarySplitRest { get; }
        public TimeSpan MinSplitRest { get; }

        public TimeSpan MaxUnbrokenDrivingLimit { get; }
        public TimeSpan MinBreakSize { get; } 
        public TimeSpan MinDrivingLimit { get; } 
        public TimeSpan MinShiftLimit { get; } 
        public TimeSpan ShiftExtensionSize { get; }
        public TimeSpan MinShiftExtensionMaxUseSize { get; }
        public TimeSpan MinOnDutyLimit { get; }
        public int NumberOfDaysInWindow { get; } 
        public TimeSpan MinWindowLimit { get; }
        public TimeSpan AdverseConditionsLimitExtension { get; }

        public List<DutyStatus> PrimaryRestDutyStatuses { get; } 
        public List<DutyStatus> SecondaryRestDutyStatuses { get; } 
        public List<DutyStatus> FullRestDutyStatuses { get; }
        public List<DutyStatus> GlobalResetDutyStatuses { get; }

        public List<DutyStatus> DrivingDutyStatuses { get; }
        public List<DutyStatus> WorkingDutyStatuses { get; }
        public List<DutyStatus> SplitRestDutyStatuses { get; }

    }

}

