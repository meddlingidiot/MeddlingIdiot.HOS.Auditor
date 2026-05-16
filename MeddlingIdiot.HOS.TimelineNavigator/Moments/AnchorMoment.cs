namespace MeddlingIdiot.HOS.TimelineNavigator.Moments
{
    [Serializable]
    public sealed class AnchorMoment : Moment
    {
        public string Comment { get; private set; }

        public AnchorMoment()
        {
            Comment = "";
        }

        public AnchorMoment(DateTime timestamp,
            string comment = "",
            string? driverIdNumber = null,
            string? truckNumber = null)
        {
            Timestamp = timestamp;
            Comment = comment;
            DriverIdNumber = driverIdNumber;
            TruckNumber = truckNumber;
        }

        public override object Clone()
        {
            var src = this;
            AnchorMoment dest = new AnchorMoment();
            dest = (AnchorMoment)PopulateClone(src, dest);
            dest.Comment = src.Comment;
            return dest;
        }
     }
}
