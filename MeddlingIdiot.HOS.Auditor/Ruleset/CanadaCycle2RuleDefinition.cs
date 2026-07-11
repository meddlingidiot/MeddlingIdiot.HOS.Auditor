using MeddlingIdiot.HOS.TimelineNavigator;

namespace MeddlingIdiot.HOS.Ruleset
{
    /// <summary>
    /// Canadian federal hours of service, south of latitude 60°N, Cycle 2
    /// (Commercial Vehicle Drivers Hours of Service Regulations, SOR/2005-313).
    ///
    /// The shift limits are identical to Cycle 1 (see <see cref="CanadaCycle1RuleDefinition"/>): 13h driving
    /// and 14h on duty before 8 consecutive hours off (s.12–13), a 16h elapsed-time driving window (s.13(3)),
    /// no 30-minute break, a +2h adverse-conditions allowance (s.76), and the s.18 sleeper split of two
    /// periods each ≥ 2h totalling ≥ 10h (<see cref="MinSplitTotalRest"/>).
    ///
    /// Cycle 2 cycle rules: no driving after 120 hours on duty in any 14 days — s.27(a) →
    /// <see cref="MinWindowLimit"/> over <see cref="NumberOfDaysInWindow"/> — and no driving after 70 hours
    /// on duty in the cycle without first taking 24 consecutive hours off — s.27(b) →
    /// <see cref="MinOnDutyLimitWithoutExtendedRest"/> / <see cref="MinExtendedRest"/>. The cycle is reset
    /// by 72 consecutive hours off — s.28(b) → <see cref="GlobalReset"/>.
    ///
    /// Daily and extended-rest requirements (same as Cycle 1): 10 hours off duty per day counting blocks
    /// ≥ 30 minutes (s.14) with s.16 deferral, and 24 consecutive hours off in the preceding 14 days (s.25).
    ///
    /// Not modelled: s.19 team splits and switching between cycles.
    /// </summary>
    public class CanadaCycle2RuleDefinition : IRuleDefinition
    {
        public string Name { get; } = "Canada Cycle 2 – 120 hours in 14 days";

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

        public int NumberOfDaysInWindow { get; } = 14;
        public TimeSpan MinWindowLimit { get; } = TimeSpan.FromHours(120);
        public TimeSpan AdverseConditionsLimitExtension { get; } = TimeSpan.FromHours(2);

        public TimeSpan MinDailyOffDuty { get; } = TimeSpan.FromHours(10);           // s.14(1)
        public TimeSpan MinDailyOffDutyBlockSize { get; } = TimeSpan.FromMinutes(30); // s.14(2)
        public TimeSpan MaxDailyOffDutyDeferral { get; } = TimeSpan.FromHours(2);     // s.16(1)(b)
        public TimeSpan MaxTwoDayDrivingWithDeferral { get; } = TimeSpan.FromHours(26); // s.16(1)(d)

        public TimeSpan MinExtendedRest { get; } = TimeSpan.FromHours(24); // s.25 and s.27(b)
        public int ExtendedRestLookbackDays { get; } = 14;                 // s.25
        public TimeSpan MinOnDutyLimitWithoutExtendedRest { get; } = TimeSpan.FromHours(70); // s.27(b)

        public List<DutyStatus> PrimaryRestDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Sleeper };
        public List<DutyStatus> SecondaryRestDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Unknown, DutyStatus.OffDuty, DutyStatus.Sleeper, DutyStatus.PersonalConveyance };
        public List<DutyStatus> FullRestDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Unknown, DutyStatus.OffDuty, DutyStatus.Sleeper, DutyStatus.PersonalConveyance };
        public List<DutyStatus> GlobalResetDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Unknown, DutyStatus.OffDuty, DutyStatus.Sleeper, DutyStatus.PersonalConveyance };

        public List<DutyStatus> DrivingDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Driving };
        public List<DutyStatus> WorkingDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Driving, DutyStatus.OnDuty, DutyStatus.YardMove };
        public List<DutyStatus> SplitRestDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Sleeper };
    }
}
