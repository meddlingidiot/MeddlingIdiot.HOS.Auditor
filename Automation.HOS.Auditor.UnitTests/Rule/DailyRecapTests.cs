using Automation.HOS.Rules;
using Automation.HOS.Ruleset;
using Automation.HOS.TimelineNavigator;
using Automation.HOS.TimelineNavigator.Moments;
using Automation.HOS.TimelineNavigator.Timelines;

namespace Automation.HOS.Auditor.UnitTests.Rule;

public class DailyRecapTests
{
    [Test]
    public async Task SeparateSegmentsIntoDays()
    {
        var nav = new TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions());
        var sut = new DailyRecap(nav);

        PopulateTimeline(nav);

        nav.JumpTo(DateTime.MinValue);
        do
        {
            if (DutyStatuses.WorkingDutyStatuses.Contains(nav.DutyStatus))
            {
                sut.Accumulate(nav.StartTimestamp, nav.Length, nav.DutyStatus);
            }
            nav.Next();
        } while (!nav.IsEndOfTime());

        await Assert.That(sut.GetValue(DateTime.Parse("03/01/2024"))).IsEqualTo(TimeSpan.FromHours(10));
        await Assert.That(sut.GetValue(DateTime.Parse("03/02/2024"))).IsEqualTo(TimeSpan.FromHours(10));
        await Assert.That(sut.GetValue(DateTime.Parse("03/03/2024"))).IsEqualTo(TimeSpan.FromHours(10));
        await Assert.That(sut.GetValue(DateTime.Parse("03/04/2024"))).IsEqualTo(TimeSpan.FromHours(10));
        await Assert.That(sut.GetValue(DateTime.Parse("03/05/2024"))).IsEqualTo(TimeSpan.FromHours(10));
        await Assert.That(sut.GetValue(DateTime.Parse("03/06/2024"))).IsEqualTo(TimeSpan.FromHours(10));
        await Assert.That(sut.GetValue(DateTime.Parse("03/07/2024"))).IsEqualTo(TimeSpan.FromHours(10));
        await Assert.That(sut.GetValue(DateTime.Parse("03/08/2024"))).IsEqualTo(TimeSpan.FromHours(10));
    }

    [Test]
    public async Task CalculateTotalUsedBeforeToday()
    {
        var nav = new TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions());
        var sut = new DailyRecap(nav);

        PopulateTimeline(nav);

        nav.JumpTo(DateTime.MinValue);
        do
        {
            if (DutyStatuses.WorkingDutyStatuses.Contains(nav.DutyStatus))
            {
                sut.Accumulate(nav.StartTimestamp, nav.Length, nav.DutyStatus);
            }
            nav.Next();
        } while (!nav.IsEndOfTime());

        await Assert.That(sut.GetTotalUsed(DateTime.Parse("03/07/2024"), 7)).IsEqualTo(TimeSpan.FromHours(60));
        await Assert.That(sut.GetTotalUsed(DateTime.Parse("03/02/2024"), 7)).IsEqualTo(TimeSpan.FromHours(10));
    }

    private static void PopulateTimeline(TimelineNavigator.TimelineNavigator nav)
    {
        nav.Add(new DutyStatusChangeMoment(DateTime.Parse("03/01/2024 08:00:00"), DutyStatus.OnDuty));
        nav.Add(new DutyStatusChangeMoment(DateTime.Parse("03/01/2024 10:00:00"), DutyStatus.Driving));
        nav.Add(new DutyStatusChangeMoment(DateTime.Parse("03/01/2024 18:00:00"), DutyStatus.OffDuty));

        nav.Add(new DutyStatusChangeMoment(DateTime.Parse("03/02/2024 08:00:00"), DutyStatus.OnDuty));
        nav.Add(new DutyStatusChangeMoment(DateTime.Parse("03/02/2024 10:00:00"), DutyStatus.Driving));
        nav.Add(new DutyStatusChangeMoment(DateTime.Parse("03/02/2024 18:00:00"), DutyStatus.OffDuty));

        nav.Add(new DutyStatusChangeMoment(DateTime.Parse("03/03/2024 08:00:00"), DutyStatus.OnDuty));
        nav.Add(new DutyStatusChangeMoment(DateTime.Parse("03/03/2024 10:00:00"), DutyStatus.Driving));
        nav.Add(new DutyStatusChangeMoment(DateTime.Parse("03/03/2024 18:00:00"), DutyStatus.OffDuty));

        nav.Add(new DutyStatusChangeMoment(DateTime.Parse("03/04/2024 08:00:00"), DutyStatus.OnDuty));
        nav.Add(new DutyStatusChangeMoment(DateTime.Parse("03/04/2024 10:00:00"), DutyStatus.Driving));
        nav.Add(new DutyStatusChangeMoment(DateTime.Parse("03/04/2024 18:00:00"), DutyStatus.OffDuty));

        nav.Add(new DutyStatusChangeMoment(DateTime.Parse("03/05/2024 08:00:00"), DutyStatus.OnDuty));
        nav.Add(new DutyStatusChangeMoment(DateTime.Parse("03/05/2024 10:00:00"), DutyStatus.Driving));
        nav.Add(new DutyStatusChangeMoment(DateTime.Parse("03/05/2024 18:00:00"), DutyStatus.OffDuty));

        nav.Add(new DutyStatusChangeMoment(DateTime.Parse("03/06/2024 08:00:00"), DutyStatus.OnDuty));
        nav.Add(new DutyStatusChangeMoment(DateTime.Parse("03/06/2024 10:00:00"), DutyStatus.Driving));
        nav.Add(new DutyStatusChangeMoment(DateTime.Parse("03/06/2024 18:00:00"), DutyStatus.OffDuty));

        nav.Add(new DutyStatusChangeMoment(DateTime.Parse("03/07/2024 08:00:00"), DutyStatus.OnDuty));
        nav.Add(new DutyStatusChangeMoment(DateTime.Parse("03/07/2024 10:00:00"), DutyStatus.Driving));
        nav.Add(new DutyStatusChangeMoment(DateTime.Parse("03/07/2024 18:00:00"), DutyStatus.OffDuty));

        nav.Add(new DutyStatusChangeMoment(DateTime.Parse("03/08/2024 08:00:00"), DutyStatus.OnDuty));
        nav.Add(new DutyStatusChangeMoment(DateTime.Parse("03/08/2024 10:00:00"), DutyStatus.Driving));
        nav.Add(new DutyStatusChangeMoment(DateTime.Parse("03/08/2024 18:00:00"), DutyStatus.OffDuty));
    }
}