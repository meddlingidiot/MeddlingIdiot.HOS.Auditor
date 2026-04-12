using MeddlingIdiot.HOS.TimelineNavigator;

namespace MeddlingIdiot.HOS;

public interface IGpsStreamToDutyStatusTimeline
{
    ITimelineNavigator ConvertGpsTimelineToDutyStatusTimeline(ITimelineNavigator gpsNavigator);
}