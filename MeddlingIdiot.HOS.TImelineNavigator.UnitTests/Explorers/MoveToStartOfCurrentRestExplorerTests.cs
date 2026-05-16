using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Explorers;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;
using MeddlingIdiot.HOS.TimelineNavigator.Timelines;

namespace MeddlingIdiot.HOS.TimelineNavigator.UnitTests.Explorers;

public class MoveToStartOfCurrentRestExplorerTests
{
    private void PopulateTimeline(ITimelineNavigator data)
    {
        data.Add(new DutyStatusChangeMoment(DateTime.Parse("02/14/2023 08:00:00"), DutyStatus.OnDuty));
        //ON DUTY 10 HOURS
        data.Add(new DutyStatusChangeMoment(DateTime.Parse("02/14/2023 18:00:00"), DutyStatus.OffDuty));
        //OFF DUTY 2 HOURS
        data.Add(new DutyStatusChangeMoment(DateTime.Parse("02/14/2023 20:00:00"), DutyStatus.Sleeper));
        //SLEEPER 1 HOUR (TOTAL REST 3 HOURS)
        data.Add(new DutyStatusChangeMoment(DateTime.Parse("02/14/2023 21:00:00"), DutyStatus.OnDuty));
        //ON DUTY 3 HOURS
        data.Add(new DutyStatusChangeMoment(DateTime.Parse("02/15/2023 00:00:00"), DutyStatus.Sleeper));
        //SLEEPER 4 HOURS
        data.Add(new DutyStatusChangeMoment(DateTime.Parse("02/15/2023 04:00:00"), DutyStatus.OnDuty));
        //ON DUTY Rest of day
        data.Add(new DutyStatusChangeMoment(DateTime.Parse("02/16/2023 00:00:00"), DutyStatus.Unknown));
        //UNKNOWN TIL EOT
    }

    [Test]
    public async Task NotMoveIfNotOnRest()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(new DateTime(0)));
        PopulateTimeline(sut);
        sut.Initialize();

        sut.JumpTo(DateTime.Parse("02/14/2023 09:00:00"));

        sut.MoveToStartOfCurrentRest();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/14/2023 08:00:00"));
    }

    [Test]
    public async Task MoveToBoTIfBeforeData()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions());
        PopulateTimeline(sut);
        sut.Initialize();

        sut.JumpTo(DateTime.Parse("02/14/2023 07:00:00"));

        sut.MoveToStartOfCurrentRest();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.MinValue);
    }

    [Test]
    public async Task MoveOffRest()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(new DateTime(0)));
        PopulateTimeline(sut);
        sut.Initialize();

        sut.JumpTo(DateTime.Parse("02/14/2023 20:00:00"));

        sut.MoveToStartOfCurrentRest();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/14/2023 18:00:00"));
    }

    [Test]
    public async Task MoveToEotIfAfterData()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(new DateTime(0)));
        PopulateTimeline(sut);
        sut.Initialize();

        sut.JumpTo(DateTime.Parse("04/14/2023 18:00:00"));

        sut.MoveToStartOfCurrentRest();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/16/2023 00:00:00"));
    }
}
