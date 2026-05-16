using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;
using MeddlingIdiot.HOS.TimelineNavigator.Timelines;

namespace MeddlingIdiot.HOS.TimelineNavigator.UnitTests.Timelines;

public class TimelineTests
{
    private void AddSomeMomentsToTimeline(Timeline<DutyStatusChangeMoment> sut)
    {
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/20/2023 01:00:00"), DutyStatus.OffDuty, "1"));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/20/2023 10:00:00"), DutyStatus.Driving, "1"));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/20/2023 21:00:00"), DutyStatus.OnDuty, "1"));
    }

    [Test]
    public async Task LoadAndWalkThruHappyPath()
    {
        var sut = new Timeline<DutyStatusChangeMoment>();
        AddSomeMomentsToTimeline(sut);

        sut.First();
        using (Assert.Multiple())
        {
            await Assert.That(sut.IsBeginningOfTime()).IsFalse();
            await Assert.That(sut.IsEndOfTime()).IsFalse();
            await Assert.That(sut.CurrentMoment.CurrentDutyStatus).IsEqualTo(DutyStatus.OffDuty);
        }
        sut.Next();
        using (Assert.Multiple())
        {
            await Assert.That(sut.IsBeginningOfTime()).IsFalse();
            await Assert.That(sut.IsEndOfTime()).IsFalse();
            await Assert.That(sut.CurrentMoment.CurrentDutyStatus).IsEqualTo(DutyStatus.Driving);
        }
        sut.Next();
        using (Assert.Multiple())
        {
            await Assert.That(sut.IsBeginningOfTime()).IsFalse();
            await Assert.That(sut.IsEndOfTime()).IsTrue();
            await Assert.That(sut.CurrentMoment.CurrentDutyStatus).IsEqualTo(DutyStatus.OnDuty);
        }
        sut.Next();
    }

    [Test]
    public async Task WalkBackwardsFromFirst()
    {
        var sut = new Timeline<DutyStatusChangeMoment>();
        AddSomeMomentsToTimeline(sut);

        sut.First();
        sut.Prior();
        using (Assert.Multiple())
        {
            await Assert.That(sut.IsBeginningOfTime()).IsTrue();
            await Assert.That(sut.IsEndOfTime()).IsFalse();
            await Assert.That(sut.CurrentMoment.CurrentDutyStatus).IsEqualTo(DutyStatus.Unknown);
        }

        sut.Next();
        using (Assert.Multiple())
        {
            await Assert.That(sut.IsBeginningOfTime()).IsFalse();
            await Assert.That(sut.IsEndOfTime()).IsFalse();
            await Assert.That(sut.CurrentMoment.CurrentDutyStatus).IsEqualTo(DutyStatus.OffDuty);
        }
    }

    [Test]
    public async Task WalkBackwardsBeyondBeginningOfTime()
    {
        var sut = new Timeline<DutyStatusChangeMoment>();
        AddSomeMomentsToTimeline(sut);

        sut.First();
        sut.Prior();
        sut.Prior();
        sut.Prior();
        sut.Prior();
        await Assert.That(sut.IsBeginningOfTime()).IsTrue();
        await Assert.That(sut.IsEndOfTime()).IsFalse();
        await Assert.That(sut.CurrentMoment.CurrentDutyStatus).IsEqualTo(DutyStatus.Unknown);
        sut.Next();
        await Assert.That(sut.IsBeginningOfTime()).IsFalse();
        await Assert.That(sut.IsEndOfTime()).IsFalse();
        await Assert.That(sut.CurrentMoment.CurrentDutyStatus).IsEqualTo(DutyStatus.OffDuty);
    }

    [Test]
    public async Task WalkBeyondTheEndOfTime()
    {
        var sut = new Timeline<DutyStatusChangeMoment>();
        AddSomeMomentsToTimeline(sut);

        sut.First();
        sut.Next();
        sut.Next();
        sut.Next();
        sut.Next();
        sut.Next();
        sut.Next();
        sut.Next();
        sut.Next();
        using (Assert.Multiple())
        {
            await Assert.That(sut.IsBeginningOfTime()).IsFalse();
            await Assert.That(sut.IsEndOfTime()).IsTrue();
            await Assert.That(sut.CurrentMoment.CurrentDutyStatus).IsEqualTo(DutyStatus.OnDuty);
        }

        sut.Prior();
        using (Assert.Multiple())
        {
            await Assert.That(sut.IsBeginningOfTime()).IsFalse();
            await Assert.That(sut.IsEndOfTime()).IsFalse();
            await Assert.That(sut.CurrentMoment.CurrentDutyStatus).IsEqualTo(DutyStatus.Driving);
        }
    }

    [Test]
    public async Task MoveOnOrBeforeHappyPath()
    {
        var sut = new Timeline<DutyStatusChangeMoment>();
        AddSomeMomentsToTimeline(sut);

        //Between Points
        sut.MoveOnOrBefore(DateTime.Parse("01/20/2023 10:30:00"));
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("01/20/2023 10:00:00"));

        //On A point
        sut.MoveOnOrBefore(DateTime.Parse("01/20/2023 10:00:00"));
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("01/20/2023 10:00:00"));

        //Before first point
        sut.MoveOnOrBefore(DateTime.Parse("01/20/2023 00:00:00"));
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.MinValue);

        //After last point
        sut.MoveOnOrBefore(DateTime.Parse("01/20/2023 23:00:00"));
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("01/20/2023 21:00:00"));
    }

    [Test]
    public async Task MoveOnOrAfterHappyPath()
    {
        var sut = new Timeline<DutyStatusChangeMoment>();
        AddSomeMomentsToTimeline(sut);

        //Between Points
        sut.MoveOnOrAfter(DateTime.Parse("01/20/2023 10:30:00"));
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("01/20/2023 21:00:00"));

        //On A point
        sut.MoveOnOrAfter(DateTime.Parse("01/20/2023 10:00:00"));
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("01/20/2023 10:00:00"));

        //Before first point
        sut.MoveOnOrAfter(DateTime.Parse("01/20/2023 00:00:00"));
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("01/20/2023 01:00:00"));

        //After last point
        sut.MoveOnOrAfter(DateTime.Parse("01/20/2023 23:00:00"));
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("01/20/2023 21:00:00"));
    }

    [Test]
    public async Task InitializeShouldSortMoments()
    {
        var sut = new Timeline<DutyStatusChangeMoment>();
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/20/2023 21:00:00"), DutyStatus.OnDuty, "1"));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/20/2023 10:00:00"), DutyStatus.Driving, "1"));
        sut.Add(new DutyStatusChangeMoment(DateTime.MinValue, DutyStatus.OffDuty, "1"));
        sut.Initialize();
        sut.First();
        sut.Next();
        await Assert.That(sut.IsBeginningOfTime()).IsFalse();
        await Assert.That(sut.IsEndOfTime()).IsFalse();
        await Assert.That(sut.CurrentMoment.CurrentDutyStatus).IsEqualTo(DutyStatus.Driving);
        sut.Next();
        await Assert.That(sut.IsBeginningOfTime()).IsFalse();
        await Assert.That(sut.IsEndOfTime()).IsTrue();
        await Assert.That(sut.CurrentMoment.CurrentDutyStatus).IsEqualTo(DutyStatus.OnDuty);
        sut.Next();
    }

    [Test]
    public async Task EmptyTimelineShouldntFail()
    {
        var sut = new Timeline<DutyStatusChangeMoment>();
        sut.Initialize();

        sut.First();
        await Assert.That(sut.BaseMoment.Timestamp).IsEqualTo(DateTime.MinValue);
    }
}
