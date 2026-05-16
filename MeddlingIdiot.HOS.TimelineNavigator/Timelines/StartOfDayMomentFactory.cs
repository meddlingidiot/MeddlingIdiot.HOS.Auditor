using MeddlingIdiot.HOS.TimelineNavigator.Moments;

namespace MeddlingIdiot.HOS.TimelineNavigator.Timelines;

internal static class StartOfDayMomentFactory
{
    public static StartOfDayMoment CreateInstance(DateTime timestamp, string? driverIdNumber, string? truckNumber)
    {
        return new StartOfDayMoment(timestamp, driverIdNumber, truckNumber);
    }
}