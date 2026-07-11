using MeddlingIdiot.HOS.TimelineNavigator;

namespace MeddlingIdiot.HOS.Ruleset
{
    /// <summary>
    /// <see cref="CanadaAutoCycleRuleDefinition"/> for a team of drivers sharing a sleeper-berth
    /// vehicle: automatic cycle election (s.26–s.29) combined with the s.19 team sleeper split —
    /// two periods, each at least 4 hours, together at least 8 hours. Under s.19 the split
    /// satisfies both s.13 and the s.14 daily minimum, so a paired-split day only needs the 8h
    /// split total. Select this ruleset when the driver is operating as part of a team and the
    /// declared cycle is unknown or may change mid-timeline.
    /// </summary>
    public class CanadaTeamAutoCycleRuleDefinition : IRuleDefinition
    {
        public string Name { get; } = "Canada Team – automatic cycle election";

        public TimeSpan GlobalReset { get; } = TimeSpan.FromHours(72);
        public TimeSpan MinFullRest { get; } = TimeSpan.FromHours(8);
        public bool UsesPrimarySplit { get; } = false;
        public TimeSpan MinPrimarySplitRest { get; } = TimeSpan.FromHours(0);
        public TimeSpan MinSecondarySplitRest { get; } = TimeSpan.FromHours(0);
        public TimeSpan MinSplitRest { get; } = TimeSpan.FromHours(4);      // s.19: each period ≥ 4h
        public TimeSpan MinSplitTotalRest { get; } = TimeSpan.FromHours(8); // s.19: together ≥ 8h

        public TimeSpan MaxUnbrokenDrivingLimit { get; } = TimeSpan.FromHours(0);
        public TimeSpan MinBreakSize { get; } = TimeSpan.FromMinutes(0);
        public TimeSpan MinDrivingLimit { get; } = TimeSpan.FromHours(13);
        public TimeSpan MinShiftLimit { get; } = TimeSpan.FromHours(16);
        public TimeSpan ShiftExtensionSize { get; } = TimeSpan.FromHours(0);
        public TimeSpan MinShiftExtensionMaxUseSize { get; } = TimeSpan.FromDays(0);
        public TimeSpan MinOnDutyLimit { get; } = TimeSpan.FromHours(14);

        // Recap display only — WindowRuleThrowsViolations mutes this rule's violations.
        public int NumberOfDaysInWindow { get; } = 14;
        public TimeSpan MinWindowLimit { get; } = TimeSpan.FromHours(120);
        public bool WindowRuleThrowsViolations { get; } = false;
        public TimeSpan AdverseConditionsLimitExtension { get; } = TimeSpan.FromHours(2);

        public TimeSpan MinDailyOffDuty { get; } = TimeSpan.FromHours(10);           // s.14(1)
        public TimeSpan MinDailyOffDutyBlockSize { get; } = TimeSpan.FromMinutes(30); // s.14(2)
        public TimeSpan MaxDailyOffDutyDeferral { get; } = TimeSpan.FromHours(2);     // s.16(1)(b)
        public TimeSpan MaxTwoDayDrivingWithDeferral { get; } = TimeSpan.FromHours(26); // s.16(1)(d)

        public TimeSpan MinExtendedRest { get; } = TimeSpan.FromHours(24); // s.25 and s.27(b)
        public int ExtendedRestLookbackDays { get; } = 14;                 // s.25
        public TimeSpan MinOnDutyLimitWithoutExtendedRest { get; } = TimeSpan.Zero; // s.27(b) handled per feasible state

        public TimeSpan Cycle1WindowLimit { get; } = TimeSpan.FromHours(70);   // s.26
        public int Cycle1DaysInWindow { get; } = 7;                            // s.26
        public TimeSpan Cycle1CycleReset { get; } = TimeSpan.FromHours(36);    // s.28(a)/s.29
        public TimeSpan Cycle2WindowLimit { get; } = TimeSpan.FromHours(120);  // s.27(a)
        public int Cycle2DaysInWindow { get; } = 14;                           // s.27(a)
        public TimeSpan Cycle2CycleReset { get; } = TimeSpan.FromHours(72);    // s.28(b)/s.29
        public TimeSpan Cycle2OnDutyLimitWithoutExtendedRest { get; } = TimeSpan.FromHours(70); // s.27(b)

        public List<DutyStatus> PrimaryRestDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Sleeper };
        public List<DutyStatus> SecondaryRestDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Unknown, DutyStatus.OffDuty, DutyStatus.Sleeper, DutyStatus.PersonalConveyance };
        public List<DutyStatus> FullRestDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Unknown, DutyStatus.OffDuty, DutyStatus.Sleeper, DutyStatus.PersonalConveyance };
        public List<DutyStatus> GlobalResetDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Unknown, DutyStatus.OffDuty, DutyStatus.Sleeper, DutyStatus.PersonalConveyance };

        public List<DutyStatus> DrivingDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Driving };
        public List<DutyStatus> WorkingDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Driving, DutyStatus.OnDuty, DutyStatus.YardMove };
        public List<DutyStatus> SplitRestDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Sleeper };
    }
}
