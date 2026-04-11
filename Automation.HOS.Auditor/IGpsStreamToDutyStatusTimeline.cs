using Automation.HOS.TimelineNavigator;

namespace Automation.HOS;

public interface IGpsStreamToDutyStatusTimeline
{
    ITimelineNavigator ConvertGpsTimelineToDutyStatusTimeline(ITimelineNavigator gpsNavigator);
}