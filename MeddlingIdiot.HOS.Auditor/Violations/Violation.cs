namespace MeddlingIdiot.HOS.Violations
{
    public class Violation
    {
        public string? DriverIdNumber { get; private set; } = null;
        public string? TruckNumber { get; private set; } = null;
        public DateTime OverLimitStartTime { get; private set; } = DateTime.MaxValue;
        public TimeSpan OverLimitTotalSize { get; private set; } = TimeSpan.Zero;
        public DateTime StartTimestamp { get; private set; } = DateTime.MaxValue;
        public TimeSpan TotalSize { get; private set; } = TimeSpan.Zero;
        public DateTime EndTimestamp => StartTimestamp.Add(TotalSize);
        public TimeSpan Limit { get; private set; } = TimeSpan.Zero;
        public TimeSpan TimeInViolation => TotalSize;
        public ViolationType ViolationType { get; private set; }
        public string Comment { get; private set; }

        public Violation(string? driverIdNumber, string? truckNumber, DateTime overLimitStartTime, TimeSpan overLimitTotalSize, DateTime startTimestamp, TimeSpan totalSize,
              TimeSpan limit, TimeSpan timeInViolation, string comment)
        {
            DriverIdNumber = driverIdNumber;
            TruckNumber = truckNumber;
            OverLimitStartTime = overLimitStartTime;
            OverLimitTotalSize = overLimitTotalSize;
            StartTimestamp = startTimestamp;
            TotalSize = totalSize;
            Limit = limit;
            ViolationType = ViolationType.HoursOfService;
            Comment = comment;
        }

        public override string ToString()
        {
            return $"{ViolationType} {OverLimitStartTime} {TotalSize} {Limit} {TimeInViolation} {Comment}";
        }
        
        
    }
}
