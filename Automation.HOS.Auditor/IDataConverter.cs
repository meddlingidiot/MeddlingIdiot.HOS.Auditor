using Automation.HOS.TimelineNavigator;

namespace Automation.HOS;

public interface IDataConverter
{
    ITimelineNavigator ConvertGpsTimelineToDutyStatusTimeline(ITimelineNavigator gpsNavigator);
}