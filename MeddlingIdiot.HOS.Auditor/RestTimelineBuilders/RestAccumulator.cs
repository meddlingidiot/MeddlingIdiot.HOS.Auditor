using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Utilities;

namespace MeddlingIdiot.HOS.RestTimelineBuilders
{
    internal class RestAccumulator 
    {
        private readonly List<DutyStatus> _restDutyStatuses;
        private readonly ITimelineNavigator _navigator;
        private readonly RestAccumulatorOptions _options;
        private readonly ILogger _logger;

        public TimeSpan TotalRestSize { get; private set; }
        public TimeSpan TotalAccumulatedSize { get; private set; }
        public DateTime StartTime { get; private set; }
        public string? DriverIdNumberOfStart { get; private set; }
        public string? TruckNumberOfStart { get; private set; }

        public RestAccumulator(ITimelineNavigator navigator, RestAccumulatorOptions options, ILogger logger) 
        {
            _navigator = navigator;
            _options = options;
            _logger = logger;
            _restDutyStatuses = DutyStatuses.AllRestDutyStatuses;

            Reset();
        }

        public void Accumulate(DateTime startTimestamp, TimeSpan toAccumulate, DutyStatus dutyStatus)
        {
            if (_options.DutyStatusesThatAccumulateTime.Contains(dutyStatus))
            {
                TotalAccumulatedSize += toAccumulate;
                StartTime = SetStartTimeIfOnFirstSegment(startTimestamp, StartTime);

            }

            if (_restDutyStatuses.Contains(dutyStatus))
            {
                _logger.Debug(LoggerCategories.Accumulators, _options.LimitSize + " - Accumulate( Total: " + TotalAccumulatedSize + " TotalRest: " + TotalRestSize + " segment: " + _navigator.Length + ")");
                TotalRestSize += toAccumulate;
  
            }
            else
            {
                _logger.Debug(LoggerCategories.Accumulators, _options.LimitSize + " - not accumulatable. Total: " + TotalAccumulatedSize);
                //Only trigger if not part of the next biggest qualified rest segment.
                if ((TotalAccumulatedSize >= _options.LimitSize) && (TotalRestSize < _options.NextBiggestRestSize))
                {
                    if (StartTime == DateTime.MaxValue)
                        throw new ArgumentOutOfRangeException(nameof(StartTime));

                    _logger.Debug(LoggerCategories.Accumulators, _options.LimitSize + " - Limit Reached!");
                    _options.OnLimitReached?.Invoke(StartTime, StartTime.Add(TotalAccumulatedSize), DriverIdNumberOfStart, TruckNumberOfStart);

                }
                Reset();

            }

        }

        public void Reset()
        {
            TotalAccumulatedSize = TimeSpan.Zero;
            TotalRestSize = TimeSpan.Zero;
            StartTime = DateTime.MaxValue;
            DriverIdNumberOfStart = null;
            TruckNumberOfStart = null;
        }

        private DateTime SetStartTimeIfOnFirstSegment(DateTime startTimestamp, DateTime startOfAccumulatedTime)
        {
            if (startOfAccumulatedTime == DateTime.MaxValue)
            {
                startOfAccumulatedTime = startTimestamp;
                DriverIdNumberOfStart = _navigator.DriverIdNumber;
                TruckNumberOfStart = _navigator.TruckNumber;
            }

            return startOfAccumulatedTime;
        }

    }
}
