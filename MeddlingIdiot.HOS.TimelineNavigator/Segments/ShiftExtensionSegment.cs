namespace MeddlingIdiot.HOS.TimelineNavigator.Segments
{
    [Serializable]
    public sealed class ShiftExtensionSegment : SegmentBase
    {
        public bool IsExtended { get; internal set; } = true;

        public ShiftExtensionSegment(DateTime startTimestamp, 
            DateTime finishTimestamp, 
            bool isExtended = true, 
            string? driverIdNumber = null,
            string? truckNumber = null) : base(startTimestamp, finishTimestamp, driverIdNumber, truckNumber)
        {
            IsExtended = isExtended;
        }

        }
}
