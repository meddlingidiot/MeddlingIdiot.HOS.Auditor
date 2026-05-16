namespace MeddlingIdiot.HOS.TimelineNavigator.Moments
{
    [Serializable]
    public sealed class ShiftExtensionMoment : Moment
    {
        public bool IsExtended { get; private set; }

        public ShiftExtensionMoment()
        {
            IsExtended = false;
        }

        public ShiftExtensionMoment(DateTime timestamp,
            bool isExtended = false,
            string? driverIdNumber = null,
            string? truckNumber = null)
        {
            Timestamp = timestamp;
            IsExtended = isExtended;
            DriverIdNumber = driverIdNumber;
            TruckNumber = truckNumber;
        }

        public override object Clone()
        {
            var src = this;
            ShiftExtensionMoment dest = new ShiftExtensionMoment();
            dest = (ShiftExtensionMoment)PopulateClone(src, dest);
            dest.IsExtended = src.IsExtended;
            return dest;
        }
    }
}