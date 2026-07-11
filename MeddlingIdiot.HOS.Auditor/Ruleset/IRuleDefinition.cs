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

        /// <summary>
        /// Minimum <em>combined</em> length the two paired sleeper-split periods must reach to
        /// count as an equivalent full rest. For US-style splits this equals <see cref="MinFullRest"/>
        /// (the two periods add back up to the single required rest), so it defaults to that and the
        /// existing rulesets need no change. Canada is the exception: a split totals at least 10 hours
        /// (SOR/2005-313 s.18) even though a single consecutive full rest is only 8 hours, so the two
        /// values genuinely differ there.
        /// </summary>
        public TimeSpan MinSplitTotalRest => MinFullRest;

        // ── Daily off-duty minimum (Canada SOR/2005-313 s.14, deferral s.16). ──
        // All default to zero, which disables the DailyOffDutyChecker entirely, so
        // rulesets without a daily off-duty requirement are unaffected.

        /// <summary>Minimum total off-duty time required in each day (Canada: 10h). Zero disables the rule.</summary>
        public TimeSpan MinDailyOffDuty => TimeSpan.Zero;
        /// <summary>Smallest off-duty block that counts toward <see cref="MinDailyOffDuty"/> (Canada: 30 minutes).</summary>
        public TimeSpan MinDailyOffDutyBlockSize => TimeSpan.Zero;
        /// <summary>Largest daily off-duty shortfall that may be deferred to the next day (Canada: 2h). Zero disables deferral.</summary>
        public TimeSpan MaxDailyOffDutyDeferral => TimeSpan.Zero;
        /// <summary>Combined two-day driving cap when a deferral is used (Canada: 26h).</summary>
        public TimeSpan MaxTwoDayDrivingWithDeferral => TimeSpan.Zero;

        // ── Extended-rest requirements (Canada s.25 and s.27(b)). ──
        // Zero values disable the ExtendedRestChecker.

        /// <summary>Length of the extended off-duty period required by the lookback and mid-cycle rules (Canada: 24h consecutive).</summary>
        public TimeSpan MinExtendedRest => TimeSpan.Zero;
        /// <summary>Days of lookback within which an extended rest must exist before driving (Canada s.25: 14). Zero disables.</summary>
        public int ExtendedRestLookbackDays => 0;
        /// <summary>On-duty time a driver may accumulate in a cycle before driving requires an extended rest
        /// (Canada Cycle 2, s.27(b): 70h). Zero disables.</summary>
        public TimeSpan MinOnDutyLimitWithoutExtendedRest => TimeSpan.Zero;

        // ── Automatic cycle election (Canada s.26–s.29 feasibility). ──
        // When both cycle window limits are non-zero, the CycleFeasibilityChecker replaces the
        // declared-cycle window rule: it tracks both cycles in parallel, applies the optional
        // resets and switches at qualifying rests, and throws only when the driver drives with
        // no compliant election left. Rulesets using it should keep the standard WindowRule for
        // day summaries but mute it via WindowRuleThrowsViolations.

        /// <summary>Cycle 1 on-duty limit (Canada s.26: 70h in 7 days). Zero disables automatic cycle election.</summary>
        public TimeSpan Cycle1WindowLimit => TimeSpan.Zero;
        public int Cycle1DaysInWindow => 0;
        /// <summary>Rest that ends a Cycle 1 cycle and permits switching to Cycle 2 (s.28(a)/s.29: 36h).</summary>
        public TimeSpan Cycle1CycleReset => TimeSpan.Zero;

        /// <summary>Cycle 2 on-duty limit (Canada s.27(a): 120h in 14 days). Zero disables automatic cycle election.</summary>
        public TimeSpan Cycle2WindowLimit => TimeSpan.Zero;
        public int Cycle2DaysInWindow => 0;
        /// <summary>Rest that ends a Cycle 2 cycle and permits switching to Cycle 1 (s.28(b)/s.29: 72h).</summary>
        public TimeSpan Cycle2CycleReset => TimeSpan.Zero;
        /// <summary>The s.27(b) gate as applied to feasible Cycle 2 states (70h; uses <see cref="MinExtendedRest"/> as the rest size).</summary>
        public TimeSpan Cycle2OnDutyLimitWithoutExtendedRest => TimeSpan.Zero;

        /// <summary>False keeps the standard window rule accumulating (day summaries for the planner UI)
        /// without reporting violations — used by automatic-cycle rulesets where no single window limit applies.</summary>
        public bool WindowRuleThrowsViolations => true;

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

