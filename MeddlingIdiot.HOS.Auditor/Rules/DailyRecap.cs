using MeddlingIdiot.HOS.TimelineNavigator;

namespace MeddlingIdiot.HOS.Rules
{
    internal class DailyRecap
    {
        private readonly Dictionary<DateTime, TimeSpan> _dailyRecap = new Dictionary<DateTime, TimeSpan>();
        private readonly ITimelineNavigator _navigator;


        public DailyRecap(ITimelineNavigator navigator)
        {
            _navigator = navigator;
        }                

        public void Accumulate(DateTime startTimestamp, TimeSpan toAccumulate, DutyStatus dutyStatus)
        {
            if (startTimestamp == DateTime.MinValue)
                return; //Don't accumulate if the start time is BOT.

            DateTime date = _navigator.StartOfDay(startTimestamp);
            if (!_dailyRecap.ContainsKey(date))
            {
                _dailyRecap[date] = TimeSpan.Zero;
            } 
            _dailyRecap[date] += toAccumulate;
        }

        public TimeSpan GetValue(DateTime index)
        { 
            return _dailyRecap[index];
        }

        internal TimeSpan GetTotalUsed(DateTime auditDay, int daysInWindow)
        {
            if (auditDay == DateTime.MinValue)
                return TimeSpan.Zero;
            var totalUsed = TimeSpan.Zero;
            for (var date = auditDay.AddDays(-daysInWindow - 1); date < auditDay; date = date.AddDays(1))
            {
                if (_dailyRecap.ContainsKey(date))
                {
                    totalUsed += _dailyRecap[date];
                }
            }
            return totalUsed;
        }

        internal void Reset()
        {
            _dailyRecap.Clear();
        }
    }
}
