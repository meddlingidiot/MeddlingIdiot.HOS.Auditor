namespace MeddlingIdiot.HOS.TimelineNavigator.Moments
{
    public abstract class Moment : ICloneable
    {
        public string? TruckNumber { get; internal set; } = null;
        public string? DriverIdNumber { get; internal set; } = null;
        public DateTime Timestamp { get; internal set; } = DateTime.MinValue;

        public abstract object Clone();

        public object PopulateClone(Moment src, Moment dest)
        {
            dest.Timestamp = src.Timestamp;
            dest.TruckNumber = src.TruckNumber;
            dest.DriverIdNumber = src.DriverIdNumber;

            return dest;

        }
    }
}
