using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Explorers;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;
using MeddlingIdiot.HOS.TimelineNavigator.Timelines;

namespace MeddlingIdiot.HOS.TimelineNavigator.UnitTests.Explorers;

public class RestFinderExplorerTests
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
    public async Task FindRestGoingForwardMoveToBeginning()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions());
        PopulateTimeline(sut);
        sut.Initialize();

        sut.JumpTo(DateTime.Parse("02/14/2023 09:00:00"));

        var found = sut.FindRest(TimeSpan.FromHours(4), TimelineDirection.Forward, PreferredEndOfRest.Beginning);
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/15/2023 00:00:00"));
            await Assert.That(found.Timestamp).IsEqualTo(sut.Start.Timestamp);
        }

        sut.JumpTo(DateTime.Parse("02/14/2023 09:00:00"));

        found = sut.FindRest(TimeSpan.FromHours(2), TimelineDirection.Forward, PreferredEndOfRest.Beginning);
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/14/2023 18:00:00"));
            await Assert.That(found.Timestamp).IsEqualTo(sut.Start.Timestamp);
        }
    }

    [Test]
    public async Task FindRestGoingForwardMoveToEnd()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions());
        PopulateTimeline(sut);
        sut.Initialize();

        sut.JumpTo(DateTime.Parse("02/14/2023 09:00:00"));

        var found = sut.FindRest(TimeSpan.FromHours(4), TimelineDirection.Forward, PreferredEndOfRest.Ending);
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/15/2023 00:00:00"));
            await Assert.That(found.Timestamp).IsEqualTo(sut.Start.Timestamp);
        }

        sut.JumpTo(DateTime.Parse("02/14/2023 09:00:00"));

        found = sut.FindRest(TimeSpan.FromHours(2), TimelineDirection.Forward, PreferredEndOfRest.Ending);
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/14/2023 20:00:00"));
            await Assert.That(found.Timestamp).IsEqualTo(sut.Start.Timestamp);
        }
    }

    [Test]
    public async Task FindRestGoingForwardNoMove()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions());
        PopulateTimeline(sut);
        sut.Initialize();

        sut.JumpTo(DateTime.Parse("02/14/2023 09:00:00"));

        var found = sut.FindRest(TimeSpan.FromHours(4), TimelineDirection.Forward, PreferredEndOfRest.Ending, MoveTo.None);
        using (Assert.Multiple())
        {
            await Assert.That(found.Timestamp).IsEqualTo(DateTime.Parse("02/15/2023 00:00:00"));
            await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/14/2023 08:00:00"));
        }

        sut.JumpTo(DateTime.Parse("02/14/2023 09:00:00"));

        found = sut.FindRest(TimeSpan.FromHours(2), TimelineDirection.Forward, PreferredEndOfRest.Ending, MoveTo.None);
        using (Assert.Multiple())
        {
            await Assert.That(found.Timestamp).IsEqualTo(DateTime.Parse("02/14/2023 20:00:00"));
            await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/14/2023 08:00:00"));
        }
    }

    [Test]
    public async Task FindRestGoingBackwardMoveToBeginning()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions());
        PopulateTimeline(sut);
        sut.Initialize();

        sut.JumpTo(DateTime.Parse("02/14/2023 22:00:00"));

        var found = sut.FindRest(TimeSpan.FromHours(4), TimelineDirection.Backward, PreferredEndOfRest.Beginning);
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.MinValue);
            await Assert.That(found.Timestamp).IsEqualTo(sut.Start.Timestamp);
        }

        sut.JumpTo(DateTime.Parse("02/14/2023 22:00:00"));

        found = sut.FindRest(TimeSpan.FromHours(2), TimelineDirection.Backward, PreferredEndOfRest.Beginning);
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/14/2023 18:00:00"));
            await Assert.That(found.Timestamp).IsEqualTo(sut.Start.Timestamp);
        }
    }

    [Test]
    public async Task FindRestGoingBackwardNoMove()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions());
        PopulateTimeline(sut);
        sut.Initialize();

        sut.JumpTo(DateTime.Parse("02/14/2023 22:00:00"));

        var found = sut.FindRest(TimeSpan.FromHours(4), TimelineDirection.Backward, PreferredEndOfRest.Ending, MoveTo.None);
        using (Assert.Multiple())
        {
            await Assert.That(found.Timestamp).IsEqualTo(DateTime.Parse("02/14/2023 00:00:00"));
            await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/14/2023 21:00:00"));
        }

        sut.JumpTo(DateTime.Parse("02/14/2023 22:00:00"));

        found = sut.FindRest(TimeSpan.FromHours(2), TimelineDirection.Backward, PreferredEndOfRest.Ending, MoveTo.None);
        using (Assert.Multiple())
        {
            await Assert.That(found.Timestamp).IsEqualTo(DateTime.Parse("02/14/2023 18:00:00"));
            await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/14/2023 21:00:00"));
        }
    }
}
