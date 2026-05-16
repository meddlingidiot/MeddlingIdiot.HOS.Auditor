namespace MeddlingIdiot.HOS.TimelineNavigator.Moments
{
    [Serializable]
    public sealed class AdverseConditionsMoment : Moment
    {
        public bool IsEnabled { get; private set; }

        public AdverseConditionsMoment()
        {
            IsEnabled = false;
        }

        public AdverseConditionsMoment(DateTime timestamp,
            bool isEnabled = false,
            string? driverIdNumber = null,
            string? truckNumber = null)
        {
            Timestamp = timestamp;
            IsEnabled = isEnabled;
            DriverIdNumber = driverIdNumber;
            TruckNumber = truckNumber;
        }

        public override object Clone()
        {
            var src = this;
            AdverseConditionsMoment dest = new AdverseConditionsMoment();
            dest = (AdverseConditionsMoment)PopulateClone(src, dest);
            dest.IsEnabled = src.IsEnabled;
            return dest;
        }
    }
}