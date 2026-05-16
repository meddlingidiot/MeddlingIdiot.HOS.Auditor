using MeddlingIdiot.HOS.TimelineNavigator.Moments;

namespace MeddlingIdiot.HOS.TimelineNavigator.Timelines
{
    [Serializable]
    public sealed class Timeline<T> : ITimeline, ITimeline<T> where T : Moment, new()
    {
        private readonly List<T> _momentList;
        private int _currentIndex;
        private readonly T _beginningOfTimeMoment;

        public Timeline()
        {
            _momentList = new List<T>();

            _beginningOfTimeMoment = Activator.CreateInstance<T>();

            Clear();
        }

        public void Clear()
        {
            _momentList.Clear();
            Add(_beginningOfTimeMoment);
        }


        public void Add(T value)
        {
            _momentList.Add(value);
        }

        public void Upsert(T value)
        {
            var foundAt = -1;
            for (var i = 0; i < _momentList.Count; i++)
            {
                if (_momentList[i].Timestamp == value.Timestamp)
                {
                    foundAt = i;
                }
            }

            if (foundAt == -1)
            {
                Add(value);
            }
            else
            {
                _momentList[foundAt] = value;
            }
        }

        public void Initialize()
        {
            List<T> sortedMomentList = new List<T>();
            sortedMomentList.AddRange(_momentList.OrderBy(m => m.Timestamp).ToList());

            _momentList.Clear();
            _momentList.AddRange(sortedMomentList);
        }

        public void First()
        {
            if (_momentList.Count <= 1)
            {
                _currentIndex = 0;
                return;
            }
            _currentIndex = 1;
        }

        public void Next()
        {
            if (!IsEndOfTime())
                _currentIndex++;
        }

        public void Prior()
        {
            if (!IsBeginningOfTime())
                _currentIndex--;
        }

        public bool IsBeginningOfTime()
        {
            return _currentIndex <= 0;
        }

        public bool IsEndOfTime()
        {
            return _currentIndex == _momentList.Count - 1;
        }

        public T CurrentMoment => _momentList[_currentIndex];

        public Moment BaseMoment => (Moment)_momentList[_currentIndex];

        public void MoveOnOrBefore(DateTime searchTimestamp)
        {
            for (var i = 0; i < _momentList.Count; i++)
            {
                if (searchTimestamp < _momentList[i].Timestamp || i == _momentList.Count - 1)
                {
                    _currentIndex = i;
                    i = _momentList.Count; // jump out of loop
                }
            }

            if (searchTimestamp < CurrentMoment.Timestamp)
            {
                _currentIndex--;
            }
        }

        public void MoveOnOrAfter(DateTime searchTimestamp)
        {
            for (var i = 0; i < _momentList.Count; i++)
            {
                if (searchTimestamp <= _momentList[i].Timestamp || i == _momentList.Count - 1)
                {
                    _currentIndex = i;
                    i = _momentList.Count; // jump out of loop
                }
            }
        }
    }
}
