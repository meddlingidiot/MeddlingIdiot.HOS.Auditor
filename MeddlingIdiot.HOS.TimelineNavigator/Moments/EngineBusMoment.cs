namespace MeddlingIdiot.HOS.TimelineNavigator.Moments
{
    [Serializable]
    public sealed class EngineBusMoment : Moment
    {
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public decimal Odometer { get; private set; }

        public EngineBusMoment() { }

        public EngineBusMoment(DateTime timestamp,
            decimal odometer,
            string? driverIdNumber = null,
            string? truckNumber = null)
        {
            Timestamp=timestamp; 
            Odometer=odometer;
            DriverIdNumber=driverIdNumber;
            TruckNumber =truckNumber;
        }

        public override object Clone()
        {
            var src = this;
            EngineBusMoment dest = new EngineBusMoment();
            dest = (EngineBusMoment)PopulateClone(src, dest);
            dest.Odometer = src.Odometer;
            return dest;
        }

        public (double, double) DistanceTo(EngineBusMoment other)
        {
            var distance = Math.Abs(other.Odometer - Odometer);
            var speed = (double)distance / (other.Timestamp - Timestamp).TotalHours;
            return ((double)distance, speed);
        }

    }
}