using MeddlingIdiot.HOS.TimelineNavigator.Moments;

namespace MeddlingIdiot.HOS.TimelineNavigator.Timelines
{
    [Serializable]
    public class StartOfDayTimeline<T> : ITimeline, ITimeline<T> where T : StartOfDayMoment
    {
        private int _daysToLoadBeforeEarliestTimestamp = 13;

        private readonly StartOfDayTimelineOptions _options;
        private T _currentMoment;
        private DateTime _earliestTimestamp;
        private DateTime _latestTimestamp;

        public StartOfDayTimeline(StartOfDayTimelineOptions options)
        {
            _options = options;
            _daysToLoadBeforeEarliestTimestamp = _options.DaysToLoadBeforeEarliestTimestamp;
            _currentMoment = (T)StartOfDayMomentFactory.CreateInstance(DateTime.MinValue, null, null);
            Clear();
        }

        public T CurrentMoment
        {
            get => _currentMoment;
            private set => _currentMoment = value;
        }

        public Moment BaseMoment => (Moment)_currentMoment;

        public void Add(T value)
        {
            if ((value.Timestamp < _earliestTimestamp) && (value.Timestamp != DateTime.MinValue))
                _earliestTimestamp = value.Timestamp;
            if ((value.Timestamp > _latestTimestamp) && (value.Timestamp != DateTime.MaxValue))
                _latestTimestamp = value.Timestamp;
        }

        public void Upsert(T value)
        {
            Add(value);
        }

        public void Initialize()
        {
            //Intentionally left blank
        }

        public void Clear()
        {
            _earliestTimestamp = DateTime.MaxValue;
            _latestTimestamp = DateTime.MinValue;

        }

        public bool IsBeginningOfTime()
        {
            return CurrentMoment.Timestamp == DateTime.MinValue;
        }

        public bool IsEndOfTime()
        {
            var latestPlusMarginTimestamp = StartOfDay(_latestTimestamp + TimeSpan.FromDays(_options.DaysToLoadBeforeEarliestTimestamp));
            return _currentMoment.Timestamp >= latestPlusMarginTimestamp;
        }

        public DateTime StartOfDay(DateTime timestamp)
        {
            return (timestamp.Date + _options.StartOfDay.TimeOfDay);
        }

        public void MoveOnOrBefore(DateTime searchTimestamp)
        {
            if (searchTimestamp < StartOfDay(_earliestTimestamp))
            {
                //CurrentMoment.Timestamp = DateTime.MinValue;
                CurrentMoment = NewMoment(DateTime.MinValue);
                return;
            }
            var latestPlusMarginTimestamp = StartOfDay(_latestTimestamp + TimeSpan.FromDays(_options.DaysToLoadBeforeEarliestTimestamp));
            if (searchTimestamp > latestPlusMarginTimestamp)
            {
                //CurrentMoment.Timestamp = latestPlusMarginTimestamp;
                CurrentMoment = NewMoment(latestPlusMarginTimestamp);
                return;
            }

            //CurrentMoment.Timestamp = StartOfDay(searchTimestamp);
            CurrentMoment = NewMoment(StartOfDay(searchTimestamp));
        }

        public void MoveOnOrAfter(DateTime searchTimestamp)
        {
            if (searchTimestamp == DateTime.MaxValue)
            {
                CurrentMoment = NewMoment(DateTime.MaxValue);
                return;
            }

            if (searchTimestamp < StartOfDay(_earliestTimestamp))
            {
                //CurrentMoment.Timestamp = StartOfDay(_earliestTimestamp);
                CurrentMoment = NewMoment(StartOfDay(_earliestTimestamp));
                return;
            }
            var latestPlusMarginTimestamp = StartOfDay(_latestTimestamp + TimeSpan.FromDays(_options.DaysToLoadBeforeEarliestTimestamp));
            if (searchTimestamp > latestPlusMarginTimestamp)
            {
                CurrentMoment = NewMoment(DateTime.MaxValue);
                return;
            }

            //Exact match
            if (searchTimestamp == StartOfDay(searchTimestamp))
            {
                //CurrentMoment.Timestamp = StartOfDay(searchTimestamp);
                CurrentMoment = NewMoment(StartOfDay(searchTimestamp));
                return;
            }

            //CurrentMoment.Timestamp = StartOfDay(searchTimestamp).AddDays(1);
            CurrentMoment = NewMoment(StartOfDay(searchTimestamp).AddDays(1));
        }

        public void Next()
        {

            //If on Beginning of time record
            if (CurrentMoment.Timestamp == DateTime.MinValue)
            {
                //CurrentMoment.Timestamp = StartOfDay(_earliestTimestamp);
                CurrentMoment = NewMoment(StartOfDay(_earliestTimestamp));
                return;
            }

            var latestPlusMarginTimestamp = StartOfDay(_latestTimestamp + TimeSpan.FromDays(_options.DaysToLoadBeforeEarliestTimestamp));
            if (CurrentMoment.Timestamp < latestPlusMarginTimestamp)
            {
                //CurrentMoment.Timestamp += TimeSpan.FromDays(1);
                CurrentMoment = NewMoment(CurrentMoment.Timestamp.AddDays(1));
            }
        }

        public void Prior()
        {
            if (CurrentMoment.Timestamp <= StartOfDay(_earliestTimestamp))
            {
                //CurrentMoment.Timestamp = DateTime.MinValue;
                CurrentMoment = NewMoment(DateTime.MinValue);
                return;
            }

            var latestPlusMarginTimestamp = StartOfDay(_latestTimestamp.Date +
                                                           TimeSpan.FromDays(_options.DaysToLoadBeforeEarliestTimestamp));
            if (CurrentMoment.Timestamp <= latestPlusMarginTimestamp)
            {
                //CurrentMoment.Timestamp -= TimeSpan.FromDays(1);
                CurrentMoment = NewMoment(CurrentMoment.Timestamp.AddDays(-1));
                return;
            }
        }

        private T NewMoment(DateTime timestamp)
        {
            var newMoment = (T)StartOfDayMomentFactory.CreateInstance(timestamp, _currentMoment.DriverIdNumber, _currentMoment.TruckNumber);
            return newMoment;
        }

        public bool IsStartOfDay(DateTime timestamp)
        {
            return timestamp == StartOfDay(timestamp);
        }
    }
}
