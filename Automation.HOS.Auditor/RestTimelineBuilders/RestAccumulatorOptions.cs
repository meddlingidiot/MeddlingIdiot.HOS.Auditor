using Automation.HOS.TimelineNavigator;

namespace Automation.HOS.RestTimelineBuilders
{
    internal class RestAccumulatorOptions
    {
        public List<DutyStatus> DutyStatusesThatAccumulateTime { get; set; }
        public TimeSpan LimitSize { get; set; }
        public TimeSpan NextBiggestRestSize { get; set; }
        public Action<DateTime, DateTime, string?, string?>? OnLimitReached { get; set; }

        public RestAccumulatorOptions(IEnumerable<DutyStatus> dutyStatusesThatAccumulateTime,
            TimeSpan limitSize, TimeSpan nextBiggestRestSize,
            Action<DateTime, DateTime, string?, string?> onLimitReached)
        {
            DutyStatusesThatAccumulateTime = new List<DutyStatus>();
            DutyStatusesThatAccumulateTime.AddRange(dutyStatusesThatAccumulateTime);
            LimitSize = limitSize;
            NextBiggestRestSize = nextBiggestRestSize;
            OnLimitReached = onLimitReached;

        }
    }
}

