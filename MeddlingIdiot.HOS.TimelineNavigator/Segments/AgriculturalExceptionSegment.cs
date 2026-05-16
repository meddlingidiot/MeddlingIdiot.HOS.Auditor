namespace MeddlingIdiot.HOS.TimelineNavigator.Segments
{
    public class AgriculturalExceptionSegment : SegmentBase
    {
        public bool IsEnabled { get; internal set; } = true;

        public AgriculturalExceptionSegment(DateTime startTimestamp,
            DateTime finishTimestamp,
            bool isEnabled = true,
            string? driverIdNumber = null,
            string? truckNumber = null) : base(startTimestamp, finishTimestamp, driverIdNumber, truckNumber)
        {
            IsEnabled = isEnabled;
        }
    }
}