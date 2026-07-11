using MeddlingIdiot.HOS.Rules;
using MeddlingIdiot.HOS.TimelineNavigator;

namespace MeddlingIdiot.HOS.Auditor.UnitTests;

// Regression guard for the DailyRecap window arithmetic. Unlike the existing
// DailyRecapTests (whose data starts exactly at the window edge, so it can't tell a
// 7-day window from a 9-day one), this fills MORE days than any window so the trailing
// count is actually exercised. A "70 hours in N days" window counts today plus the
// (N-1) days before it, so GetTotalUsed (the "before today" portion) must return
// exactly (N-1) prior days — not N+1.
public class DailyRecapWindowArithmeticTests
{
    [Test]
    public async Task GetTotalUsed_CountsExactlyDaysInWindowMinusOnePriorDays()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        var recap = new DailyRecap(navigator);

        // 1 hour of on-duty on each of 20 consecutive days, so every look-back lands on data.
        for (int d = 1; d <= 20; d++)
            recap.Accumulate(new DateTime(2024, 1, d, 0, 0, 0), TimeSpan.FromHours(1), DutyStatus.OnDuty);

        var auditDay = new DateTime(2024, 1, 15);

        using (Assert.Multiple())
        {
            // 7-day window -> 6 prior days.
            await Assert.That(recap.GetTotalUsed(auditDay, 7)).IsEqualTo(TimeSpan.FromHours(6));
            // 14-day window (Canada Cycle 2) -> 13 prior days.
            await Assert.That(recap.GetTotalUsed(auditDay, 14)).IsEqualTo(TimeSpan.FromHours(13));
            // 1-day window -> today only, so zero prior days.
            await Assert.That(recap.GetTotalUsed(auditDay, 1)).IsEqualTo(TimeSpan.Zero);
        }
    }
}
