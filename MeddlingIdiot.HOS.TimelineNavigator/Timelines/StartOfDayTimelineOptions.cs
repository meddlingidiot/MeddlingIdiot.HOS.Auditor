namespace MeddlingIdiot.HOS.TimelineNavigator.Timelines
{
    public class StartOfDayTimelineOptions
    {
        public DateTime StartOfDay { get; private set; }
        public bool StopAtStartOfDay { get; private set; }
        public int DaysToLoadBeforeEarliestTimestamp { get; private set; }
        
        public StartOfDayTimelineOptions()
        {
            StartOfDay = new DateTime(0);
            StopAtStartOfDay = true;
            DaysToLoadBeforeEarliestTimestamp = 13;
        }

        public StartOfDayTimelineOptions(DateTime? startOfDay = null, bool stopAtStartOfDay = true, int daysToLoadBeforeEarliestTimestamp = 13)
        {
            if (startOfDay == null)
                startOfDay = new DateTime(0);
            StartOfDay = startOfDay.Value;
            StopAtStartOfDay = stopAtStartOfDay;
            DaysToLoadBeforeEarliestTimestamp = daysToLoadBeforeEarliestTimestamp;
        }
    }
}
