namespace MeddlingIdiot.HOS.Queries
{
    public class DaySummary
    {
        public DateTime Date { get; }
        public TimeSpan HoursForDay { get; }
        public TimeSpan HoursInWindow { get; }
        public TimeSpan HoursAvailableToday { get; }
        public TimeSpan HoursAvailableTomorrow { get; }
        internal IReadOnlyDictionary<DateTime, TimeSpan> DailyHours { get; }

        internal DaySummary(DateTime date, TimeSpan hoursForDay, TimeSpan hoursInWindow,
            TimeSpan windowLimit, int daysInWindow,
            IReadOnlyDictionary<DateTime, TimeSpan> dailyHours)
        {
            Date = date;
            HoursForDay = hoursForDay;
            HoursInWindow = hoursInWindow;
            DailyHours = dailyHours;

            HoursAvailableToday = hoursInWindow >= windowLimit
                ? TimeSpan.Zero
                : windowLimit - hoursInWindow;

            var oldestDay = date.AddDays(-daysInWindow - 1);
            var hoursOnOldestDay = dailyHours.TryGetValue(oldestDay, out var h) ? h : TimeSpan.Zero;
            var hoursInWindowTomorrow = hoursInWindow - hoursOnOldestDay;
            HoursAvailableTomorrow = hoursInWindowTomorrow >= windowLimit
                ? TimeSpan.Zero
                : windowLimit - hoursInWindowTomorrow;
        }
    }
}
