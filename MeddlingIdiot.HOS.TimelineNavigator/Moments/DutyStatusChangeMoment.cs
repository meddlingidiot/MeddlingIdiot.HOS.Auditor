namespace MeddlingIdiot.HOS.TimelineNavigator.Moments
{
    [Serializable]
    public sealed class DutyStatusChangeMoment : Moment
    {
        public DutyStatus CurrentDutyStatus { get; private set; }
        public string? Location { get; private set; }
        public double? Latitude { get; private set; }
        public double? Longitude { get; private set; }
        
        public string? Comment { get; private set; }

        public DutyStatusChangeMoment()
        {
            CurrentDutyStatus = DutyStatus.Unknown;
        }

        public DutyStatusChangeMoment(
            DateTime timestamp, 
            DutyStatus dutyStatus, 
            string? comment = null, 
            string? location = null,
            double? latitude = null,
            double? longitude = null, 
            string? driverIdNumber = null,
            string? truckNumber = null)
        {
            Timestamp = timestamp;
            CurrentDutyStatus = dutyStatus;
            Comment = comment;
            Location = location;
            Latitude = latitude;
            Longitude = longitude;
            DriverIdNumber = driverIdNumber;
            TruckNumber = truckNumber;
        }

        public override object Clone()
        {
            var src = this;
            DutyStatusChangeMoment dest = new DutyStatusChangeMoment();
            dest = (DutyStatusChangeMoment)PopulateClone(src, dest);
            dest.CurrentDutyStatus = src.CurrentDutyStatus;
            dest.Comment = src.Comment;
            dest.Location = src.Location;
            dest.Latitude = src.Latitude;
            dest.Longitude = src.Longitude;
            return dest;
        }

        public override string ToString()
        {
            return $"TS:{Timestamp} DS:{CurrentDutyStatus} C:{Comment} L:{Location} Lat:{Latitude} Long:{Longitude}";
        }
    }

}