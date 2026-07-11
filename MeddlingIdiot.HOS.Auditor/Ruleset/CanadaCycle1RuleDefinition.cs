using MeddlingIdiot.HOS.TimelineNavigator;

namespace MeddlingIdiot.HOS.Ruleset
{
    /// <summary>
    /// Canadian federal hours of service, south of latitude 60°N, Cycle 1
    /// (Commercial Vehicle Drivers Hours of Service Regulations, SOR/2005-313).
    ///
    /// Shift limits (apply to both cycles):
    ///   • 13 hours driving before 8 consecutive hours off — s.12(1), s.13(1) → <see cref="MinDrivingLimit"/> / <see cref="MinFullRest"/>.
    ///   • 14 hours on duty before 8 consecutive hours off  — s.12(2), s.13(2) → <see cref="MinOnDutyLimit"/>.
    ///   • No driving after 16 hours have elapsed since the last 8-hour off-duty break (wall clock) — s.13(3) → <see cref="MinShiftLimit"/>.
    ///   • No mandatory 30-minute break, so <see cref="MaxUnbrokenDrivingLimit"/> is zero.
    ///   • Adverse driving conditions add up to 2 hours — s.76 → <see cref="AdverseConditionsLimitExtension"/>.
    ///   • Sleeper-berth split: two periods, each ≥ 2 hours, together ≥ 10 hours — s.18 →
    ///     <see cref="MinSplitRest"/> with <see cref="MinSplitTotalRest"/> = 10h (larger than the 8h consecutive full rest).
    ///
    /// Cycle 1: no driving after 70 hours on duty in any 7 days — s.26 → <see cref="MinWindowLimit"/> over
    /// <see cref="NumberOfDaysInWindow"/>. The cycle is reset by 36 consecutive hours off — s.28(a) → <see cref="GlobalReset"/>.
    ///
    /// Daily and extended-rest requirements:
    ///   • 10 hours off duty per day, counting blocks ≥ 30 minutes — s.14 → <see cref="MinDailyOffDuty"/> /
    ///     <see cref="MinDailyOffDutyBlockSize"/>, with s.16 deferral (≤ 2h to the next day, two-day driving
    ///     ≤ 26h) → <see cref="MaxDailyOffDutyDeferral"/> / <see cref="MaxTwoDayDrivingWithDeferral"/>.
    ///   • 24 consecutive hours off in the preceding 14 days — s.25 → <see cref="MinExtendedRest"/> /
    ///     <see cref="ExtendedRestLookbackDays"/>.
    ///
    /// Not modelled: s.19 team splits and switching to Cycle 2. See
    /// <see cref="CanadaCycle2RuleDefinition"/> for the 14-day cycle.
    /// </summary>
    public class CanadaCycle1RuleDefinition : IRuleDefinition
    {
        public string Name { get; } = "Canada Cycle 1 – 70 hours in 7 days";

        public TimeSpan GlobalReset { get; } = TimeSpan.FromHours(36);
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

        public int NumberOfDaysInWindow { get; } = 7;
        public TimeSpan MinWindowLimit { get; } = TimeSpan.FromHours(70);
        public TimeSpan AdverseConditionsLimitExtension { get; } = TimeSpan.FromHours(2);

        public TimeSpan MinDailyOffDuty { get; } = TimeSpan.FromHours(10);           // s.14(1)
        public TimeSpan MinDailyOffDutyBlockSize { get; } = TimeSpan.FromMinutes(30); // s.14(2)
        public TimeSpan MaxDailyOffDutyDeferral { get; } = TimeSpan.FromHours(2);     // s.16(1)(b)
        public TimeSpan MaxTwoDayDrivingWithDeferral { get; } = TimeSpan.FromHours(26); // s.16(1)(d)

        public TimeSpan MinExtendedRest { get; } = TimeSpan.FromHours(24); // s.25
        public int ExtendedRestLookbackDays { get; } = 14;                 // s.25
        public TimeSpan MinOnDutyLimitWithoutExtendedRest { get; } = TimeSpan.Zero; // s.27(b) is Cycle 2 only

        public List<DutyStatus> PrimaryRestDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Sleeper };
        public List<DutyStatus> SecondaryRestDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Unknown, DutyStatus.OffDuty, DutyStatus.Sleeper, DutyStatus.PersonalConveyance };
        public List<DutyStatus> FullRestDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Unknown, DutyStatus.OffDuty, DutyStatus.Sleeper, DutyStatus.PersonalConveyance };
        public List<DutyStatus> GlobalResetDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Unknown, DutyStatus.OffDuty, DutyStatus.Sleeper, DutyStatus.PersonalConveyance };

        public List<DutyStatus> DrivingDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Driving };
        public List<DutyStatus> WorkingDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Driving, DutyStatus.OnDuty, DutyStatus.YardMove };
        public List<DutyStatus> SplitRestDutyStatuses { get; } = new List<DutyStatus> { DutyStatus.Sleeper };
    }
}
