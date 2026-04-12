using MeddlingIdiot.HOS.Rules;
using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;

namespace MeddlingIdiot.HOS.RuleLoop;

internal abstract class RuleLoop
{
    public abstract void Accumulate(TimeSpan toAccumulate, DutyStatus dutyStatus);
    public abstract void Reset();
    public abstract void GlobalReset();
    public abstract void ThrowViolations(ThrowViolations firedAt);
    public abstract void MainLoop(Moment startOfAuditWindow, Moment endOfAuditWindow);
}