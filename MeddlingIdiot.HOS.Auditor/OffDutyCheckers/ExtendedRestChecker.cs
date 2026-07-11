using MeddlingIdiot.HOS.Ruleset;
using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;
using MeddlingIdiot.HOS.TimelineNavigator.Utilities;
using MeddlingIdiot.HOS.Violations;

namespace MeddlingIdiot.HOS.OffDutyCheckers
{
    /// <summary>
    /// Enforces the two Canadian extended-rest requirements (SOR/2005-313):
    ///
    /// s.25 lookback — no driving unless the driver has taken at least
    /// <see cref="IRuleDefinition.MinExtendedRest"/> of consecutive off-duty time within the
    /// preceding <see cref="IRuleDefinition.ExtendedRestLookbackDays"/> days. A rest run of length
    /// L ≥ MinExtendedRest ending at E keeps a driver compliant while the sliding lookback window
    /// still contains MinExtendedRest of it, i.e. until E + lookback − MinExtendedRest. Driving
    /// past every such horizon is a violation. The segment before the first recorded moment counts
    /// as rest (unknown history is not held against the driver), which gives a natural grace
    /// period of (lookback − MinExtendedRest) after the data begins.
    ///
    /// s.27(b) mid-cycle — no driving after accumulating
    /// <see cref="IRuleDefinition.MinOnDutyLimitWithoutExtendedRest"/> of on-duty time in the
    /// current cycle unless an extended rest has been taken since the cycle began. Rest runs of at
    /// least <see cref="IRuleDefinition.GlobalReset"/> start a new cycle (accumulation and the
    /// extended-rest flag both reset); shorter runs of at least MinExtendedRest satisfy the
    /// requirement for the remainder of the cycle.
    /// </summary>
    internal class ExtendedRestChecker
    {
        private readonly ITimelineNavigator _navigator;
        private readonly IRuleDefinition _ruleDefinition;
        private readonly IViolationGateway _violationGateway;
        private readonly ILogger _logger;

        public ExtendedRestChecker(ITimelineNavigator navigator, IRuleDefinition ruleDefinition,
            IViolationGateway violationGateway, ILogger logger)
        {
            _navigator = navigator;
            _ruleDefinition = ruleDefinition;
            _violationGateway = violationGateway;
            _logger = logger;
        }

        public void MainLoop(Moment startOfAuditWindow, Moment endOfAuditWindow, CancellationToken cancellationToken = default)
        {
            _logger.Debug(LoggerCategories.Rule, "EXTENDED REST AUDIT");

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

            if (cancellationToken.IsCancellationRequested)
                return;

            if (_ruleDefinition.ExtendedRestLookbackDays > 0)
                CheckLookback(segments, restRuns, analysisStart, analysisEnd);

            if (_ruleDefinition.MinOnDutyLimitWithoutExtendedRest > TimeSpan.Zero)
                CheckMidCycle(segments, restRuns, analysisStart, analysisEnd);
        }

        private void CheckLookback(List<DutySegment> segments, List<RestRun> restRuns,
            DateTime analysisStart, DateTime analysisEnd)
        {
            var lookback = TimeSpan.FromDays(_ruleDefinition.ExtendedRestLookbackDays);
            var minExtendedRest = _ruleDefinition.MinExtendedRest;

            // Compliance horizons, in chronological order of run end.
            var horizons = restRuns
                .Where(run => run.Length >= minExtendedRest && run.End != DateTime.MaxValue)
                .Select(run => (RunEnd: run.End, Expiry: run.End + lookback - minExtendedRest))
                .ToList();

            var comment = $"No {Violation.FormatHours(minExtendedRest)} hour off-duty period in the preceding " +
                          $"{_ruleDefinition.ExtendedRestLookbackDays} days";
            var horizonIndex = 0;
            var horizon = DateTime.MinValue;

            foreach (var segment in segments)
            {
                if (!_ruleDefinition.DrivingDutyStatuses.Contains(segment.DutyStatus))
                    continue;

                while (horizonIndex < horizons.Count && horizons[horizonIndex].RunEnd <= segment.Start)
                {
                    if (horizons[horizonIndex].Expiry > horizon)
                        horizon = horizons[horizonIndex].Expiry;
                    horizonIndex++;
                }

                var start = Max(segment.Start, analysisStart);
                var end = Min(segment.Finish, analysisEnd);
                if (end <= start)
                    continue;

                var violationStart = Max(start, horizon);
                if (end <= violationStart)
                    continue;

                _violationGateway.SaveViolation(new Violation(
                    segment.DriverIdNumber,
                    segment.TruckNumber,
                    violationStart,
                    end - violationStart,
                    violationStart,
                    end - violationStart,
                    minExtendedRest,
                    end - violationStart,
                    comment));
            }
        }

        private void CheckMidCycle(List<DutySegment> segments, List<RestRun> restRuns,
            DateTime analysisStart, DateTime analysisEnd)
        {
            var trigger = _ruleDefinition.MinOnDutyLimitWithoutExtendedRest;
            var minExtendedRest = _ruleDefinition.MinExtendedRest;
            var comment = $"Over {Violation.FormatHours(trigger)} hours on duty without " +
                          $"{Violation.FormatHours(minExtendedRest)} consecutive hours off";

            var onDutyAccumulated = TimeSpan.Zero;
            var hasExtendedRest = false;
            var runIndex = 0;

            foreach (var segment in segments)
            {
                // Apply every rest run that has completed by the start of this segment.
                while (runIndex < restRuns.Count && restRuns[runIndex].End <= segment.Start)
                {
                    var run = restRuns[runIndex];
                    if (run.Length >= _ruleDefinition.GlobalReset)
                    {
                        onDutyAccumulated = TimeSpan.Zero;
                        hasExtendedRest = false; // The extended rest must be taken within the new cycle.
                    }
                    else if (run.Length >= minExtendedRest)
                    {
                        hasExtendedRest = true;
                    }
                    runIndex++;
                }

                if (!_ruleDefinition.WorkingDutyStatuses.Contains(segment.DutyStatus))
                    continue;

                // The trailing segment runs to MaxValue; it lies past analysisEnd, so its
                // length contributes nothing that could still be audited.
                var segmentFinish = segment.Finish == DateTime.MaxValue ? segment.Start : segment.Finish;
                var accumulatedAtStart = onDutyAccumulated;
                onDutyAccumulated += segmentFinish - segment.Start;

                if (hasExtendedRest || !_ruleDefinition.DrivingDutyStatuses.Contains(segment.DutyStatus))
                    continue;

                // Driving is a violation from the moment accumulation reaches the trigger.
                if (onDutyAccumulated <= trigger)
                    continue;

                var crossing = accumulatedAtStart >= trigger
                    ? segment.Start
                    : segment.Start + (trigger - accumulatedAtStart);

                var start = Max(Max(segment.Start, crossing), analysisStart);
                var end = Min(segmentFinish, analysisEnd);
                if (end <= start)
                    continue;

                _violationGateway.SaveViolation(new Violation(
                    segment.DriverIdNumber,
                    segment.TruckNumber,
                    start,
                    end - start,
                    start,
                    end - start,
                    trigger,
                    end - start,
                    comment));
            }
        }

        private static DateTime Max(DateTime a, DateTime b) => a > b ? a : b;
        private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;
    }
}
