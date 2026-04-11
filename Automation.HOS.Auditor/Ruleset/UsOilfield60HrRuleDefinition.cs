using Automation.HOS.TimelineNavigator;

namespace Automation.HOS.Ruleset
{
    public class UsOilfield60HrRuleDefinition : IRuleDefinition
    {
        public string Name { get; } = "US Oilfield 60 hours in 7 days";

        public TimeSpan GlobalReset { get; } = TimeSpan.FromHours(34);
        public TimeSpan MinFullRest { get; } = TimeSpan.FromHours(10);
        public bool UsesPrimarySplit { get; } = false;
        public TimeSpan MinPrimarySplitRest { get; } = TimeSpan.FromHours(7);
        public TimeSpan MinSecondarySplitRest { get; } = TimeSpan.FromHours(2);
        public TimeSpan MinSplitRest { get; } = TimeSpan.FromHours(2);

        public TimeSpan MaxUnbrokenDrivingLimit { get; } = TimeSpan.FromHours(8);
        public TimeSpan MinBreakSize { get; } = TimeSpan.FromMinutes(30);
        public TimeSpan MinDrivingLimit { get; } = TimeSpan.FromHours(11);
        public TimeSpan MinShiftLimit { get; } = TimeSpan.FromHours(14);
        public TimeSpan ShiftExtensionSize { get; } = TimeSpan.FromHours(2);
        public TimeSpan MinShiftExtensionMaxUseSize { get; } = TimeSpan.FromDays(7);
        public TimeSpan MinOnDutyLimit { get; } = TimeSpan.FromHours(0);

        public int NumberOfDaysInWindow { get; } = 7;
        public TimeSpan MinWindowLimit { get; } = TimeSpan.FromHours(60);
        public TimeSpan AdverseConditionsLimitExtension { get; } = TimeSpan.FromHours(2);

        public List<DutyStatus> PrimaryRestDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Sleeper };
        public List<DutyStatus> SecondaryRestDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Unknown, DutyStatus.OffDuty, DutyStatus.Sleeper };
        public List<DutyStatus> FullRestDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Unknown, DutyStatus.OffDuty, DutyStatus.Sleeper };
        public List<DutyStatus> GlobalResetDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Unknown, DutyStatus.OffDuty, DutyStatus.Sleeper };

        public List<DutyStatus> DrivingDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Driving };
        public List<DutyStatus> WorkingDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Driving, DutyStatus.OnDuty };
        public List<DutyStatus> SplitRestDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Sleeper, DutyStatus.OffDutyWaitingAtWellSite };

    }
}
