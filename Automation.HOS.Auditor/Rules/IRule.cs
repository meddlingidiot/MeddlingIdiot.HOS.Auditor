using Automation.HOS.TimelineNavigator;

namespace Automation.HOS.Rules;

internal interface IRule
{
    void Accumulate(DateTime startTimestamp, TimeSpan toAccumulate, DutyStatus dutyStatus);
    void GlobalReset();
    void Reset();


}