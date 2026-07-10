using MeddlingIdiot.HOS.TimelineNavigator;

namespace MeddlingIdiot.HOS.Ruleset
{
    /// <summary>
    /// Florida intrastate property carriers (Fla. Stat. §316.302, non-placarded):
    /// 12 hours driving after 10 consecutive hours off duty, no driving after the
    /// end of the 16th hour after coming on duty (wall-clock window), 70 hours in
    /// 7 consecutive days, 34-hour restart. Florida does not adopt the federal
    /// 30-minute break for intrastate drivers. Sleeper splits use the old
    /// two-period style: total the full rest, neither period under 2 hours.
    /// See <see cref="FloridaIntrastate80HrRuleDefinition"/> for carriers that
    /// operate every day of the week (80 in 8).
    /// </summary>
    public class FloridaIntrastate70HrRuleDefinition : IRuleDefinition
    {
        public string Name { get; } = "Florida Intrastate 70 hours in 7 days";

        public TimeSpan GlobalReset { get; } = TimeSpan.FromHours(34);
        public TimeSpan MinFullRest { get; } = TimeSpan.FromHours(10);
        public bool UsesPrimarySplit { get; } = false;
        public TimeSpan MinPrimarySplitRest { get; } = TimeSpan.FromHours(0);
        public TimeSpan MinSecondarySplitRest { get; } = TimeSpan.FromHours(0);
        public TimeSpan MinSplitRest { get; } = TimeSpan.FromHours(2);

        public TimeSpan MaxUnbrokenDrivingLimit { get; } = TimeSpan.FromHours(0);
        public TimeSpan MinBreakSize { get; } = TimeSpan.FromMinutes(0);
        public TimeSpan MinDrivingLimit { get; } = TimeSpan.FromHours(12);
        public TimeSpan MinShiftLimit { get; } = TimeSpan.FromHours(16);
        public TimeSpan ShiftExtensionSize { get; } = TimeSpan.FromHours(0);
        public TimeSpan MinShiftExtensionMaxUseSize { get; } = TimeSpan.FromDays(0);
        public TimeSpan MinOnDutyLimit { get; } = TimeSpan.FromHours(0);

        public int NumberOfDaysInWindow { get; } = 7;
        public TimeSpan MinWindowLimit { get; } = TimeSpan.FromHours(70);
        public TimeSpan AdverseConditionsLimitExtension { get; } = TimeSpan.FromHours(2);

        public List<DutyStatus> PrimaryRestDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Sleeper };
        public List<DutyStatus> SecondaryRestDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Unknown, DutyStatus.OffDuty, DutyStatus.Sleeper, DutyStatus.PersonalConveyance };
        public List<DutyStatus> FullRestDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Unknown, DutyStatus.OffDuty, DutyStatus.Sleeper, DutyStatus.PersonalConveyance };
        public List<DutyStatus> GlobalResetDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Unknown, DutyStatus.OffDuty, DutyStatus.Sleeper, DutyStatus.PersonalConveyance };

        public List<DutyStatus> DrivingDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Driving };
        public List<DutyStatus> WorkingDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Driving, DutyStatus.OnDuty, DutyStatus.YardMove };
        public List<DutyStatus> SplitRestDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Sleeper, DutyStatus.OffDutyWaitingAtWellSite };
    }
}
