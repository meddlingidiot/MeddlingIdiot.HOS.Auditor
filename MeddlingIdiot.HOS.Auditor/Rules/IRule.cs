using MeddlingIdiot.HOS.TimelineNavigator;

namespace MeddlingIdiot.HOS.Rules;

internal interface IRule
{
    void Accumulate(DateTime startTimestamp, TimeSpan toAccumulate, DutyStatus dutyStatus);
    void GlobalReset();
    void Reset();


}