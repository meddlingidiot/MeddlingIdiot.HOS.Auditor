using MeddlingIdiot.HOS.Ruleset;
using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;
using MeddlingIdiot.HOS.TimelineNavigator.Utilities;
using MeddlingIdiot.HOS.Violations;

namespace MeddlingIdiot.HOS.OffDutyCheckers
{
    /// <summary>
    /// Automatic cycle election (SOR/2005-313 s.26–s.29): audits the timeline against every cycle
    /// declaration the driver could legally have made, and throws only when driving occurs with no
    /// compliant election left.
    ///
    /// The reachable declarations collapse into a tiny state set:
    ///   • One Cycle 1 state — s.26's rolling window (70h / 7 days) counts on-duty hours after the
    ///     state's anchor. Any rest ≥ <see cref="IRuleDefinition.Cycle1CycleReset"/> (36h) moves the
    ///     anchor forward (resetting is optional but never worse, so only the latest anchor matters).
    ///   • Up to two Cycle 2 states — s.27(a)'s rolling window (120h / 14 days) plus the s.27(b)
    ///     gate: after accumulating <see cref="IRuleDefinition.Cycle2OnDutyLimitWithoutExtendedRest"/>
    ///     (70h) since the anchor, driving requires a rest ≥ <see cref="IRuleDefinition.MinExtendedRest"/>
    ///     (24h) taken since the anchor. A rest ≥ 36h also permits switching Cycle 1 → Cycle 2
    ///     (s.29), producing a fresh Cycle 2 state; because s.28 resets are optional, an old state
    ///     with the gate satisfied and a fresh state with fewer counted hours can both be worth
    ///     keeping — the Pareto frontier over (anchor, gate) is at most two states.
    /// A rest ≥ <see cref="IRuleDefinition.Cycle2CycleReset"/> (72h) additionally covers the
    /// Cycle 2 → Cycle 1 switch, which the Cycle 1 anchor update already models (72h ≥ 36h).
    ///
    /// Windows are day-granular ("any period of 7/14 days", where a day is the carrier-designated
    /// 24h period), matching the engine's DailyRecap semantics: at any instant the window covers
    /// today-so-far plus the previous N−1 whole days, never counting hours before the state anchor.
    /// The unknown history before the first recorded moment counts as rest, so a fresh timeline
    /// starts with every election feasible and the Cycle 2 gate satisfied — consistent with how the
    /// other checkers give the benefit of the doubt at the beginning of time.
    /// </summary>
    internal class CycleFeasibilityChecker
    {
        private readonly ITimelineNavigator _navigator;
        private readonly IRuleDefinition _ruleDefinition;
        private readonly IViolationGateway _violationGateway;
        private readonly ILogger _logger;

        private sealed class Cycle2State
        {
            public DateTime Anchor;
            public bool HasExtendedRest;
        }

        public CycleFeasibilityChecker(ITimelineNavigator navigator, IRuleDefinition ruleDefinition,
            IViolationGateway violationGateway, ILogger logger)
        {
            _navigator = navigator;
            _ruleDefinition = ruleDefinition;
            _violationGateway = violationGateway;
            _logger = logger;
        }

        public void MainLoop(Moment startOfAuditWindow, Moment endOfAuditWindow, CancellationToken cancellationToken = default)
        {
            _logger.Debug(LoggerCategories.Rule, "CYCLE FEASIBILITY AUDIT");

            var segments = DutySegmentWalker.Walk(_navigator);
            if (segments.Count < 2)
                return;

            var lastRealMoment = segments[^1].Start;
            var analysisStart = startOfAuditWindow.Timestamp == DateTime.MinValue
                ? segments[0].Finish
                : startOfAuditWindow.Timestamp;
            var analysisEnd = endOfAuditWindow.Timestamp < lastRealMoment
                ? endOfAuditWindow.Timestamp
                : lastRealMoment;

            var restRuns = DutySegmentWalker.CollectRestRuns(segments, _ruleDefinition.FullRestDutyStatuses);
            var ledger = new OnDutyLedger(segments, _ruleDefinition.WorkingDutyStatuses, lastRealMoment);

            var c1Anchor = DateTime.MinValue;
            var c2States = new List<Cycle2State> { new() { Anchor = DateTime.MinValue, HasExtendedRest = false } };
            var runIndex = 0;

            var pieces = new List<(DateTime Start, DateTime End, string? DriverIdNumber, string? TruckNumber)>();

            foreach (var segment in segments)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                // Apply every rest that completed by the start of this segment.
                while (runIndex < restRuns.Count && restRuns[runIndex].End <= segment.Start)
                {
                    var run = restRuns[runIndex];
                    runIndex++;

                    if (run.Length >= _ruleDefinition.MinExtendedRest)
                        foreach (var state in c2States)
                            state.HasExtendedRest = true;

                    if (run.Length >= _ruleDefinition.Cycle1CycleReset)
                    {
                        c1Anchor = run.End; // Cycle 1 reset; also covers the 72h Cycle 2 → Cycle 1 switch.
                        c2States.Add(new Cycle2State { Anchor = run.End, HasExtendedRest = false }); // Cycle 1 → Cycle 2 switch / optional Cycle 2 reset.
                        PruneCycle2States(c2States);
                    }
                }

                if (!_ruleDefinition.DrivingDutyStatuses.Contains(segment.DutyStatus))
                    continue;

                var segmentFinish = segment.Finish == DateTime.MaxValue ? segment.Start : segment.Finish;
                var sliceStart = Max(segment.Start, analysisStart);
                var segmentEnd = Min(segmentFinish, analysisEnd);

                while (sliceStart < segmentEnd)
                {
                    var dayStart = _navigator.StartOfDay(sliceStart);
                    var sliceEnd = Min(dayStart.AddDays(1), segmentEnd);

                    var compliantUntil = CompliantUntilCycle1(ledger, c1Anchor, dayStart, sliceStart, sliceEnd);
                    foreach (var state in c2States)
                    {
                        var until = CompliantUntilCycle2(ledger, state, dayStart, sliceStart, sliceEnd);
                        if (until > compliantUntil)
                            compliantUntil = until;
                    }

                    if (compliantUntil < sliceEnd)
                        pieces.Add((compliantUntil, sliceEnd, segment.DriverIdNumber, segment.TruckNumber));

                    sliceStart = sliceEnd;
                }
            }

            EmitMerged(pieces);
        }

        /// <summary>
        /// How long into [sliceStart, sliceEnd) — a driving stretch within one day — the Cycle 1
        /// election stays compliant. Within the slice the window's lower bound is fixed, so the
        /// counted total grows linearly with the driving.
        /// </summary>
        private DateTime CompliantUntilCycle1(OnDutyLedger ledger, DateTime anchor,
            DateTime dayStart, DateTime sliceStart, DateTime sliceEnd)
        {
            var windowLow = Max(dayStart.AddDays(-(_ruleDefinition.Cycle1DaysInWindow - 1)), anchor);
            var counted = ledger.Between(windowLow, sliceStart);
            var headroom = _ruleDefinition.Cycle1WindowLimit - counted;
            if (headroom <= TimeSpan.Zero)
                return sliceStart;
            return Min(sliceStart + headroom, sliceEnd);
        }

        /// <summary>Same as Cycle 1, plus the s.27(b) gate for states that have not yet taken their extended rest.</summary>
        private DateTime CompliantUntilCycle2(OnDutyLedger ledger, Cycle2State state,
            DateTime dayStart, DateTime sliceStart, DateTime sliceEnd)
        {
            var windowLow = Max(dayStart.AddDays(-(_ruleDefinition.Cycle2DaysInWindow - 1)), state.Anchor);
            var counted = ledger.Between(windowLow, sliceStart);
            var headroom = _ruleDefinition.Cycle2WindowLimit - counted;
            var until = headroom <= TimeSpan.Zero ? sliceStart : Min(sliceStart + headroom, sliceEnd);

            if (!state.HasExtendedRest && _ruleDefinition.Cycle2OnDutyLimitWithoutExtendedRest > TimeSpan.Zero)
            {
                var accumulated = ledger.Between(state.Anchor, sliceStart);
                var gateHeadroom = _ruleDefinition.Cycle2OnDutyLimitWithoutExtendedRest - accumulated;
                var gateUntil = gateHeadroom <= TimeSpan.Zero ? sliceStart : Min(sliceStart + gateHeadroom, sliceEnd);
                if (gateUntil < until)
                    until = gateUntil;
            }

            return until;
        }

        /// <summary>Keeps the Pareto frontier over (anchor, gate satisfied): at most one state per gate value,
        /// and a gate-unsatisfied state only survives with a strictly later anchor.</summary>
        private static void PruneCycle2States(List<Cycle2State> states)
        {
            Cycle2State? bestWithRest = null;
            Cycle2State? bestWithout = null;
            foreach (var state in states)
            {
                if (state.HasExtendedRest)
                {
                    if (bestWithRest == null || state.Anchor > bestWithRest.Anchor)
                        bestWithRest = state;
                }
                else if (bestWithout == null || state.Anchor > bestWithout.Anchor)
                {
                    bestWithout = state;
                }
            }

            states.Clear();
            if (bestWithRest != null)
                states.Add(bestWithRest);
            if (bestWithout != null && (bestWithRest == null || bestWithout.Anchor > bestWithRest.Anchor))
                states.Add(bestWithout);
        }

        private void EmitMerged(List<(DateTime Start, DateTime End, string? DriverIdNumber, string? TruckNumber)> pieces)
        {
            var comment = $"Over the cycle limits under every cycle election " +
                          $"(Cycle 1 {Violation.FormatHours(_ruleDefinition.Cycle1WindowLimit)} in {_ruleDefinition.Cycle1DaysInWindow} days, " +
                          $"Cycle 2 {Violation.FormatHours(_ruleDefinition.Cycle2WindowLimit)} in {_ruleDefinition.Cycle2DaysInWindow} days)";

            for (var i = 0; i < pieces.Count; i++)
            {
                var (start, end, driverIdNumber, truckNumber) = pieces[i];
                while (i + 1 < pieces.Count && pieces[i + 1].Start <= end)
                {
                    end = Max(end, pieces[i + 1].End);
                    i++;
                }

                _violationGateway.SaveViolation(new Violation(
                    driverIdNumber,
                    truckNumber,
                    start,
                    end - start,
                    start,
                    end - start,
                    _ruleDefinition.Cycle1WindowLimit,
                    end - start,
                    comment));
            }
        }

        /// <summary>Cumulative on-duty time as a piecewise-linear function of time, for exact interval sums.</summary>
        private sealed class OnDutyLedger
        {
            private readonly List<(DateTime Start, DateTime End, TimeSpan CumulativeAtStart)> _spans = new();

            public OnDutyLedger(List<DutySegment> segments, List<DutyStatus> workingDutyStatuses, DateTime lastRealMoment)
            {
                var cumulative = TimeSpan.Zero;
                foreach (var segment in segments)
                {
                    if (!workingDutyStatuses.Contains(segment.DutyStatus))
                        continue;
                    var end = segment.Finish == DateTime.MaxValue ? lastRealMoment : segment.Finish;
                    if (end <= segment.Start)
                        continue;
                    _spans.Add((segment.Start, end, cumulative));
                    cumulative += end - segment.Start;
                }
            }

            public TimeSpan Between(DateTime from, DateTime to)
            {
                return to <= from ? TimeSpan.Zero : CumulativeAt(to) - CumulativeAt(from);
            }

            private TimeSpan CumulativeAt(DateTime timestamp)
            {
                // Last span starting at or before the timestamp.
                int low = 0, high = _spans.Count - 1, found = -1;
                while (low <= high)
                {
                    var mid = (low + high) / 2;
                    if (_spans[mid].Start <= timestamp)
                    {
                        found = mid;
                        low = mid + 1;
                    }
                    else
                    {
                        high = mid - 1;
                    }
                }

                if (found < 0)
                    return TimeSpan.Zero;

                var span = _spans[found];
                var effective = timestamp < span.End ? timestamp : span.End;
                return span.CumulativeAtStart + (effective - span.Start);
            }
        }

        private static DateTime Max(DateTime a, DateTime b) => a > b ? a : b;
        private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;
    }
}
