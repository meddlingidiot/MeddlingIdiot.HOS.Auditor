using MeddlingIdiot.HOS.TimelineNavigator.Moments;
using MeddlingIdiot.HOS.TimelineNavigator.Utilities;

namespace MeddlingIdiot.HOS.TimelineNavigator.Explorers
{
    public static class RestExplorerExtensions 
    {
        public static bool IsRest(this ITimelineNavigator timelineNavigator)
        {
            return timelineNavigator.DutyStatus is DutyStatus.OffDuty or DutyStatus.Sleeper or DutyStatus.Unknown;
        }

        public static void MoveOffRest(this ITimelineNavigator timelineNavigator, TimelineDirection direction, ILogger? logger = null) 
        {
            if (direction == TimelineDirection.Forward)
            {
                if (logger == null) logger = new NullLogger();
                
                logger.Debug("RestExplorer", $"Moving forward off rest: Timestamp: {timelineNavigator.StartTimestamp} IsRest: {timelineNavigator.IsRest()} && IsEndOfTime: {timelineNavigator.IsEndOfTime()}");
                while ((timelineNavigator.IsRest()) && (!timelineNavigator.IsEndOfTime()))
                {
                    timelineNavigator.Next();
                    logger.Debug("RestExplorer", "  - Next: " + timelineNavigator.StartTimestamp.ToString("o"));
                    logger.Debug("RestExplorer", $"Moving forward off rest: Timestamp: {timelineNavigator.StartTimestamp} IsRest: {timelineNavigator.IsRest()} && IsEndOfTime: {timelineNavigator.IsEndOfTime()}");
                }
            }
            else
            {
                while ((timelineNavigator.IsRest()) && (!timelineNavigator.IsBeginningOfTime()))
                {
                    timelineNavigator.Prior();
                }
            }
        }

        public static void MoveToStartOfCurrentRest(this ITimelineNavigator timelineNavigator)
        {
            var bookmark = (Moment)timelineNavigator.Start.Clone();

            timelineNavigator.MoveOffRest(TimelineDirection.Backward);
            if (timelineNavigator.Start.Timestamp == bookmark.Timestamp)
            {
                //We are not on rest exit..
                return;
            }
            if ((!timelineNavigator.IsRest())) 
            {
                timelineNavigator.Next();
            }
        }

        public static void MoveToEndOfCurrentRest(this ITimelineNavigator timelineNavigator, ILogger? logger = null)
        {
            logger ??= new NullLogger();
            var bookmark = (Moment)timelineNavigator.Start.Clone();

            timelineNavigator.MoveOffRest(TimelineDirection.Forward, logger);
            if (timelineNavigator.Start.Timestamp == bookmark.Timestamp)
            {
                //We are not on rest exit..
                return;
            }
            if ((!timelineNavigator.IsRest()))
            {
                timelineNavigator.Prior();
            }
        }

        private static TimeSpan AccumulateIfRest(bool isRest, TimeSpan segmentLength, TimeSpan accumulatedRest)
        {
            return isRest ? accumulatedRest.Add(segmentLength) : TimeSpan.Zero;
        }

        public static Moment FindRest(this ITimelineNavigator timelineNavigator, TimeSpan minToFind, TimelineDirection direction = TimelineDirection.Forward, PreferredEndOfRest preferredEndOfRest = PreferredEndOfRest.Ending, MoveTo moveTo = MoveTo.NewLocation)
        {
            var bookmark = (Moment)timelineNavigator.Start.Clone();

            if (direction == TimelineDirection.Forward)
            {
                timelineNavigator.MoveToStartOfCurrentRest();

                var accumulatedRest = AccumulateIfRest(timelineNavigator.IsRest(), timelineNavigator.Length, TimeSpan.Zero);
                while ((accumulatedRest < minToFind) && (!timelineNavigator.IsEndOfTime()))
                {
                    timelineNavigator.Next();
                    accumulatedRest = AccumulateIfRest(timelineNavigator.IsRest(), timelineNavigator.Length, accumulatedRest);
                }

                if (preferredEndOfRest == PreferredEndOfRest.Ending)
                {
                    timelineNavigator.MoveToEndOfCurrentRest();
                }

                var foundMoment = (Moment)timelineNavigator.Start.Clone();

                if (moveTo == MoveTo.None)
                {
                    timelineNavigator.JumpTo(bookmark.Timestamp);
                }

                return foundMoment;
            }
            else
            {
                timelineNavigator.MoveToEndOfCurrentRest();

                var accumulatedRest = AccumulateIfRest(timelineNavigator.IsRest(), timelineNavigator.Length, TimeSpan.Zero);
                while ((accumulatedRest < minToFind) && (!timelineNavigator.IsBeginningOfTime()))
                {
                    timelineNavigator.Prior();
                    accumulatedRest = AccumulateIfRest(timelineNavigator.IsRest(), timelineNavigator.Length, accumulatedRest);
                }

                if (preferredEndOfRest == PreferredEndOfRest.Beginning)
                {
                    timelineNavigator.MoveToStartOfCurrentRest();
                }

                var foundMoment = (Moment)timelineNavigator.Start.Clone();

                if (moveTo == MoveTo.None)
                {
                    timelineNavigator.JumpTo(bookmark.Timestamp);
                }

                return foundMoment;
            }
        }

    }
}
