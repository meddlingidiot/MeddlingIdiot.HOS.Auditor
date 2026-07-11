using MeddlingIdiot.HOS.TimelineNavigator;

namespace MeddlingIdiot.HOS.Ruleset
{
    /// <summary>
    /// Canadian federal hours of service, south of latitude 60°N, with <em>automatic cycle
    /// election</em> (SOR/2005-313 s.26–s.29): instead of auditing against one declared cycle,
    /// the CycleFeasibilityChecker tracks Cycle 1 (70h/7 days, 36h reset) and Cycle 2
    /// (120h/14 days + the s.27(b) 70h gate, 72h reset) in parallel, applies the optional
    /// resets and switches at qualifying rests, and throws a cycle violation only when the
    /// driver drives with no compliant election left. Use this when the driver's declared
    /// cycle is unknown or may change mid-timeline; use
    /// <see cref="CanadaCycle1RuleDefinition"/> / <see cref="CanadaCycle2RuleDefinition"/>
    /// to hold a driver to a specific declaration.
    ///
    /// Shift, daily off-duty (s.14/s.16), split (s.18), and 24h-in-14-days (s.25) rules are
    /// identical to the declared-cycle definitions. <see cref="MinWindowLimit"/> is configured
    /// as Cycle 2's 120h/14 days purely for the day-summary recap (the most permissive
    /// election); <see cref="WindowRuleThrowsViolations"/> mutes it so all cycle violations
    /// come from the feasibility checker. <see cref="MinOnDutyLimitWithoutExtendedRest"/> is
    /// zero because the s.27(b) gate only binds while Cycle 2 is the surviving election —
    /// the feasibility checker carries it per state.
    ///
    /// Not modelled: s.19 team splits.
    /// </summary>
    public class CanadaAutoCycleRuleDefinition : IRuleDefinition
    {
        public string Name { get; } = "Canada – automatic cycle election";

        public TimeSpan GlobalReset { get; } = TimeSpan.FromHours(72);
        public TimeSpan MinFullRest { get; } = TimeSpan.FromHours(8);
        public bool UsesPrimarySplit { get; } = false;
        public TimeSpan MinPrimarySplitRest { get; } = TimeSpan.FromHours(0);
        public TimeSpan MinSecondarySplitRest { get; } = TimeSpan.FromHours(0);
        public TimeSpan MinSplitRest { get; } = TimeSpan.FromHours(2);
        public TimeSpan MinSplitTotalRest { get; } = TimeSpan.FromHours(10);

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
