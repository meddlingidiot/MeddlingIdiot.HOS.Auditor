using MeddlingIdiot.HOS.TimelineNavigator;

namespace MeddlingIdiot.HOS.OffDutyCheckers
{
    /// <summary>One contiguous stretch of a single duty status, as reported by the navigator</summary>
    internal sealed record DutySegment(
        DateTime Start,
        DateTime Finish,
        DutyStatus DutyStatus,
        bool IsAdverse,
        string? DriverIdNumber,
        string? TruckNumber)
    {
        public TimeSpan OverlapWith(DateTime windowStart, DateTime windowEnd)
        {
            var start = Start > windowStart ? Start : windowStart;
            var end = Finish < windowEnd ? Finish : windowEnd;
            return end > start ? end - start : TimeSpan.Zero;
        }
    }

    /// <summary>A maximal run of consecutive rest-status segments.</summary>
    internal sealed record RestRun(DateTime Start, DateTime End, string? DriverIdNumber, string? TruckNumber)
    {
        public TimeSpan Length => End - Start;

        public TimeSpan OverlapWith(DateTime windowStart, DateTime windowEnd)
        {
            var start = Start > windowStart ? Start : windowStart;
            var end = End < windowEnd ? End : windowEnd;
            return end > start ? end - start : TimeSpan.Zero;
        }
    }

    internal static class DutySegmentWalker
    {
        /// <summary>
        /// Walks the whole timeline into a flat segment list, including the leading
        /// beginning-of-time segment and the trailing segment that runs to
        /// DateTime.MaxValue (which the usual do/Next() loop never visits).
        /// </summary>
        public static List<DutySegment> Walk(ITimelineNavigator navigator)
        {
            var segments = new List<DutySegment>();
            navigator.JumpTo(DateTime.MinValue);
            do
            {
                segments.Add(Capture(navigator));
                navigator.Next();
            } while (!navigator.IsEndOfTime());

            // The position we exited on is the final [lastMoment, MaxValue) segment.
            if (segments.Count == 0 || segments[^1].Start != navigator.StartTimestamp)
                segments.Add(Capture(navigator));

            return segments;
        }

        private static DutySegment Capture(ITimelineNavigator navigator) =>
            new(navigator.StartTimestamp, navigator.FinishTimestamp, navigator.DutyStatus,
                navigator.IsAdverseConditionsEnabled, navigator.DriverIdNumber, navigator.TruckNumber);

        /// <summary>Merges consecutive rest-status segments into maximal rest runs, in chronological order.</summary>
        public static List<RestRun> CollectRestRuns(List<DutySegment> segments, List<DutyStatus> restDutyStatuses)
        {
            var runs = new List<RestRun>();
            DateTime? runStart = null;
            string? driverIdNumber = null;
            string? truckNumber = null;

            foreach (var segment in segments)
            {
                if (restDutyStatuses.Contains(segment.DutyStatus))
                {
                    if (runStart == null)
                    {
                        runStart = segment.Start;
                        driverIdNumber = segment.DriverIdNumber;
                        truckNumber = segment.TruckNumber;
                    }
                }
                else if (runStart != null)
                {
                    runs.Add(new RestRun(runStart.Value, segment.Start, driverIdNumber, truckNumber));
                    runStart = null;
                    driverIdNumber = null;
                    truckNumber = null;
                }
            }

            if (runStart != null)
                runs.Add(new RestRun(runStart.Value, DateTime.MaxValue, driverIdNumber, truckNumber));

            return runs;
        }
    }
}
