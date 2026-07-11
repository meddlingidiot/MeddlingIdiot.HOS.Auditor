using MeddlingIdiot.HOS.Ruleset;
using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;
using MeddlingIdiot.HOS.TimelineNavigator.Utilities;
using MeddlingIdiot.HOS.Violations;

namespace MeddlingIdiot.HOS.OffDutyCheckers
{
    /// <summary>
    /// Enforces the daily off-duty minimum (SOR/2005-313 s.14): at least
    /// <see cref="IRuleDefinition.MinDailyOffDuty"/> of off-duty time in each day, counting only
    /// blocks of at least <see cref="IRuleDefinition.MinDailyOffDutyBlockSize"/> (s.14(2)). In
    /// adverse driving conditions the daily requirement drops by
    /// <see cref="IRuleDefinition.AdverseConditionsLimitExtension"/> (s.76(2)(b)).
    ///
    /// A short day is excused when a legal deferral to the next day exists (s.16): the shortfall is
    /// at most <see cref="IRuleDefinition.MaxDailyOffDutyDeferral"/>, the short day still contains its
    /// full <see cref="IRuleDefinition.MinFullRest"/> (the deferred time may not come out of the
    /// mandatory consecutive rest), the two-day off-duty total reaches twice the daily minimum, the
    /// next day has a consecutive rest long enough to absorb the deferred time
    /// (<see cref="IRuleDefinition.MinFullRest"/> + shortfall), the two-day driving total stays within
    /// <see cref="IRuleDefinition.MaxTwoDayDrivingWithDeferral"/>, and neither day uses a sleeper
    /// split. The s.16(1)(e) log declaration cannot be observed from the timeline, so the audit is
    /// lenient there: any timeline consistent with a legal deferral is not flagged.
    ///
    /// Days that end after the audit window (or after the last recorded moment) are skipped — a
    /// partial day cannot be proven short yet.
    /// </summary>
    internal class DailyOffDutyChecker
    {
        private readonly ITimelineNavigator _navigator;
        private readonly IRuleDefinition _ruleDefinition;
        private readonly IViolationGateway _violationGateway;
        private readonly ILogger _logger;

        public DailyOffDutyChecker(ITimelineNavigator navigator, IRuleDefinition ruleDefinition,
            IViolationGateway violationGateway, ILogger logger)
        {
            _navigator = navigator;
            _ruleDefinition = ruleDefinition;
            _violationGateway = violationGateway;
            _logger = logger;
        }

        public void MainLoop(Moment startOfAuditWindow, Moment endOfAuditWindow, CancellationToken cancellationToken = default)
        {
            _logger.Debug(LoggerCategories.Rule, "DAILY OFF-DUTY AUDIT");

            var segments = DutySegmentWalker.Walk(_navigator);
            if (segments.Count < 2)
                return; // Nothing but the beginning-of-time segment.

            var lastRealMoment = segments[^1].Start;
            var analysisStart = startOfAuditWindow.Timestamp == DateTime.MinValue
                ? segments[0].Finish
                : startOfAuditWindow.Timestamp;
            var analysisEnd = endOfAuditWindow.Timestamp < lastRealMoment
                ? endOfAuditWindow.Timestamp
                : lastRealMoment;

            var restRuns = DutySegmentWalker.CollectRestRuns(segments, _ruleDefinition.FullRestDutyStatuses);
            var qualifyingRuns = restRuns
                .Where(run => run.Length >= _ruleDefinition.MinDailyOffDutyBlockSize)
                .ToList();
            var pairedSplitTimestamps = _navigator.GetRestTimelineMoments()
                .Where(moment => moment.IsPaired)
                .Select(moment => moment.Timestamp)
                .ToList();

            for (var day = _navigator.StartOfDay(analysisStart);
                 day.AddDays(1) <= analysisEnd && !cancellationToken.IsCancellationRequested;
                 day = day.AddDays(1))
            {
                var dayEnd = day.AddDays(1);
                var offToday = SumRestOverlap(qualifyingRuns, day, dayEnd);

                var required = _ruleDefinition.MinDailyOffDuty;
                if (HasAdverseConditions(segments, day, dayEnd))
                    required -= _ruleDefinition.AdverseConditionsLimitExtension;

                if (offToday >= required)
                    continue;

                if (DeferralExcuses(day, offToday, required, segments, qualifyingRuns, restRuns,
                        pairedSplitTimestamps, analysisEnd))
                {
                    _logger.Debug(LoggerCategories.Rule, $"Daily off-duty shortfall on {day:d} excused by deferral.");
                    continue;
                }

                var shortfall = required - offToday;
                var (driverIdNumber, truckNumber) = FirstWorkingIdentity(segments, day, dayEnd);
                _violationGateway.SaveViolation(new Violation(
                    driverIdNumber,
                    truckNumber,
                    day,
                    shortfall,
                    day,
                    shortfall,
                    _ruleDefinition.MinDailyOffDuty,
                    shortfall,
                    $"Under {Violation.FormatHours(required)} hour daily off-duty minimum"));
            }
        }

        private bool DeferralExcuses(DateTime day, TimeSpan offToday, TimeSpan required,
            List<DutySegment> segments, List<RestRun> qualifyingRuns, List<RestRun> restRuns,
            List<DateTime> pairedSplitTimestamps, DateTime analysisEnd)
        {
            if (_ruleDefinition.MaxDailyOffDutyDeferral == TimeSpan.Zero)
                return false;

            var shortfall = required - offToday;
            if (shortfall > _ruleDefinition.MaxDailyOffDutyDeferral)
                return false;

            // The deferred time may not be part of the mandatory consecutive rest (s.16(1)(b)).
            if (offToday < _ruleDefinition.MinFullRest)
                return false;

            var nextDay = day.AddDays(1);
            var nextDayEnd = nextDay.AddDays(1);

            // No sleeper split on either day (s.16(1)(a)).
            if (pairedSplitTimestamps.Any(t => t >= day && t < nextDayEnd))
                return false;

            // The next day is incomplete: a legal deferral is still possible, so stay lenient.
            if (nextDayEnd > analysisEnd)
                return true;

            // Two-day off-duty total must reach two full daily minimums (s.16(1)(c): 20h).
            var offNextDay = SumRestOverlap(qualifyingRuns, nextDay, nextDayEnd);
            if (offToday + offNextDay < _ruleDefinition.MinDailyOffDuty + _ruleDefinition.MinDailyOffDuty)
                return false;

            // The deferred time is added to the next day's consecutive rest (s.16(1)(c)).
            var requiredConsecutive = _ruleDefinition.MinFullRest + shortfall;
            if (!restRuns.Any(run => run.End > nextDay && run.Start < nextDayEnd && run.Length >= requiredConsecutive))
                return false;

            // Combined driving over the two days (s.16(1)(d): 26h).
            var twoDayDriving = SumStatusOverlap(segments, _ruleDefinition.DrivingDutyStatuses, day, nextDayEnd);
            if (twoDayDriving > _ruleDefinition.MaxTwoDayDrivingWithDeferral)
                return false;

            return true;
        }

        private static TimeSpan SumRestOverlap(List<RestRun> runs, DateTime windowStart, DateTime windowEnd)
        {
            var total = TimeSpan.Zero;
            foreach (var run in runs)
                total += run.OverlapWith(windowStart, windowEnd);
            return total;
        }

        private static TimeSpan SumStatusOverlap(List<DutySegment> segments, List<DutyStatus> dutyStatuses,
            DateTime windowStart, DateTime windowEnd)
        {
            var total = TimeSpan.Zero;
            foreach (var segment in segments)
            {
                if (dutyStatuses.Contains(segment.DutyStatus))
                    total += segment.OverlapWith(windowStart, windowEnd);
            }
            return total;
        }

        private static bool HasAdverseConditions(List<DutySegment> segments, DateTime windowStart, DateTime windowEnd)
        {
            return segments.Any(segment => segment.IsAdverse && segment.OverlapWith(windowStart, windowEnd) > TimeSpan.Zero);
        }

        private (string? DriverIdNumber, string? TruckNumber) FirstWorkingIdentity(
            List<DutySegment> segments, DateTime windowStart, DateTime windowEnd)
        {
            var working = segments.FirstOrDefault(segment =>
                _ruleDefinition.WorkingDutyStatuses.Contains(segment.DutyStatus) &&
                segment.OverlapWith(windowStart, windowEnd) > TimeSpan.Zero);
            return (working?.DriverIdNumber, working?.TruckNumber);
        }
    }
}
