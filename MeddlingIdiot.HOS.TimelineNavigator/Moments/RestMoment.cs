namespace MeddlingIdiot.HOS.TimelineNavigator.Moments
{
    [Serializable]
    public sealed class RestMoment : Moment
    {
        public TimeSpan Duration { get; private set; }
        public bool IsGlobalReset { get; private set; }
        public bool IsFullRest { get; private set; }
        public bool IsQualified { get; private set; }
        public bool IsPrimary { get; private set; }
        public bool IsPaired { get; private set; }

        public RestMoment() { }

        public RestMoment(DateTime timestamp,
            TimeSpan duration,
            bool isGlobalReset = false,
            bool isFullRest = false,
            bool isQualified = false, 
            bool isPrimary = false,
            bool isPaired = false,
            string? driverIdNumber = null,
            string? truckNumber = null)
        {
            Timestamp = timestamp;
            Duration = duration;
            IsGlobalReset = isGlobalReset;
            IsFullRest = isFullRest;
            IsQualified = isQualified;
            IsPrimary = isPrimary;
            IsPaired = isPaired;
            DriverIdNumber = driverIdNumber;
            TruckNumber = truckNumber;
        }


        public override object Clone()
        {
            var src = this;
            RestMoment dest = new RestMoment();
            dest = (RestMoment)PopulateClone(src, dest);
            dest.Duration = src.Duration;
            dest.IsGlobalReset = src.IsGlobalReset;
            dest.IsFullRest = src.IsFullRest;
            dest.IsQualified = src.IsQualified;
            dest.IsPrimary = src.IsPrimary;
            dest.IsPaired = src.IsPaired;
            return dest;

        }

        public override string ToString()
        {
            return $"TS:{Timestamp} GlobalReset:{IsGlobalReset} FullRest: {IsFullRest} Qualified:{IsQualified} Primary:{IsPrimary} Paired:{IsPaired}";
        }
    }
}
