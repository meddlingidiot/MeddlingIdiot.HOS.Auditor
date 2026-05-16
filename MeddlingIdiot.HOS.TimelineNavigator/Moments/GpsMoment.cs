namespace MeddlingIdiot.HOS.TimelineNavigator.Moments
{
    [Serializable]
    public sealed class GpsMoment : Moment
    {
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public double? Altitude { get; private set; }
        public double? Speed { get; private set; }
        public double? Bearing { get; private set; }
        
        public string? StringRepresentation { get; private set; }

        public GpsMoment() { }

        public GpsMoment(DateTime timestamp,
            double latitude,
            double longitude,
            string? stringRepresentation = null,
            double? altitude = null,
            double? speed = null,
            double? bearing = null,
            string? driverIdNumber = null,
            string? truckNumber = null)
        {
            Timestamp=timestamp; 
            Latitude=latitude;
            Longitude=longitude;
            StringRepresentation=stringRepresentation;
            Altitude=altitude;
            Speed=speed;
            Bearing=bearing;
            DriverIdNumber=driverIdNumber;
            TruckNumber =truckNumber;
        }

        public override object Clone()
        {
            var src = this;
            GpsMoment dest = new GpsMoment();
            dest = (GpsMoment)PopulateClone(src, dest);
            dest.Latitude = src.Latitude;
            dest.Longitude = src.Longitude;
            dest.StringRepresentation = src.StringRepresentation;
            dest.Altitude = src.Altitude;
            dest.Speed = src.Speed;
            dest.Bearing = src.Bearing;
            dest.DriverIdNumber = src.DriverIdNumber;
            dest.TruckNumber = src.TruckNumber;
            return dest;
        }

        public (double, double) DistanceTo(GpsMoment other)
        {
            var distance = Math.Sqrt(Math.Pow(Latitude - other.Latitude, 2) + Math.Pow(Longitude - other.Longitude, 2));
            var speed = distance / (other.Timestamp - Timestamp).TotalHours;
            return (distance, speed);
        }

    }
}
