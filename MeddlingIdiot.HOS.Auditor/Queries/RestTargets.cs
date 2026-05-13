namespace MeddlingIdiot.HOS.Queries
{
    public class SleeperRestTargets
    {
        public TimeSpan MinSplitRest { get; }
        public TimeSpan MinPrimarySplitRest { get; }
        public TimeSpan MinFullRest { get; }
        public TimeSpan GlobalReset { get; }

        public SleeperRestTargets(TimeSpan minSplitRest, TimeSpan minPrimarySplitRest, TimeSpan minFullRest, TimeSpan globalReset)
        {
            MinSplitRest = minSplitRest;
            MinPrimarySplitRest = minPrimarySplitRest;
            MinFullRest = minFullRest;
            GlobalReset = globalReset;
        }
    }

    public class OffDutyRestTargets
    {
        public TimeSpan MinFullRest { get; }
        public TimeSpan GlobalReset { get; }

        public OffDutyRestTargets(TimeSpan minFullRest, TimeSpan globalReset)
        {
            MinFullRest = minFullRest;
            GlobalReset = globalReset;
        }
    }

    public class RestTargets
    {
        public SleeperRestTargets Sleeper { get; }
        public OffDutyRestTargets OffDuty { get; }

        public RestTargets(SleeperRestTargets sleeper, OffDutyRestTargets offDuty)
        {
            Sleeper = sleeper;
            OffDuty = offDuty;
        }
    }
}
