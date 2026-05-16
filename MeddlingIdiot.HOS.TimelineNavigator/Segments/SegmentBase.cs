namespace MeddlingIdiot.HOS.TimelineNavigator.Segments
{
    public class SegmentBase
    {
        public string? TruckNumber { get; internal set; } = null;
        public string? DriverIdNumber { get; internal set; } = null;
        public DateTime StartTimestamp { get; internal set; } = DateTime.MinValue;
        public DateTime FinishTimestamp { get; internal set; } = DateTime.MinValue;

        public SegmentBase(DateTime startTimestamp, DateTime finishTimestamp, string? driverIdNumber = null,
            string? truckNumber = null)
        {
            TruckNumber = truckNumber;
            DriverIdNumber = driverIdNumber;
            StartTimestamp = startTimestamp;
            FinishTimestamp = finishTimestamp;
        }
    }
}