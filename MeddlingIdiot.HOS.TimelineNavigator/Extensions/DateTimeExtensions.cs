namespace MeddlingIdiot.HOS.TimelineNavigator.Extensions
{
    public static class DateTimeExtensions
    {
        public static TimeSpan AbsoluteDifference(this DateTime start, DateTime end)
        {
            return start > end ? start - end : end - start;
        }
    }
}
