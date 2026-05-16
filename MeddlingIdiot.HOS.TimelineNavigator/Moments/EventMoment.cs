namespace MeddlingIdiot.HOS.TimelineNavigator.Moments
{
    [Serializable]
    public sealed class EventMoment : Moment
    {
        public int? EventCode { get; private set; }
        public string? Comment { get; private set; }
        

        public EventMoment() { }

        public EventMoment(DateTime timestamp,
            int? eventCode = null,
            string? comment = null,
            string? driverIdNumber = null,
            string? truckNumber = null
            )
        {
            Timestamp = timestamp;
            EventCode = eventCode;
            Comment = comment;
            DriverIdNumber = driverIdNumber;
            TruckNumber = truckNumber;
        }

        public override object Clone()
        {
            var src = this;
            EventMoment dest = new EventMoment();
            dest = (EventMoment)PopulateClone(src, dest);
            dest.EventCode = src.EventCode;
            dest.Comment = src.Comment;
            dest.DriverIdNumber = src.DriverIdNumber;
            dest.TruckNumber = src.TruckNumber;
            return dest;
        }

    }
}