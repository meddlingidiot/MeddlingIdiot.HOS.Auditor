using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Explorers;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;
using MeddlingIdiot.HOS.TimelineNavigator.Timelines;

namespace MeddlingIdiot.HOS.TimelineNavigator.UnitTests.Timelines;

public class StartOfDayTimelineTests
{
    [Test]
    public async Task JumpByOneDayNextHappyPath()
    {
        var sut = new StartOfDayTimeline<StartOfDayMoment>(new StartOfDayTimelineOptions(DateTime.MinValue));

        sut.Add(new StartOfDayMoment(DateTime.Parse("01/29/2023 00:00:00")));
        sut.Add(new StartOfDayMoment(DateTime.Parse("01/27/2023 00:00:00")));
        sut.MoveOnOrBefore(DateTime.Parse("01/26/2023 03:32:31 PM"));
        await Assert.That(sut.IsBeginningOfTime()).IsTrue();
        await Assert.That(sut.IsEndOfTime()).IsFalse();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.MinValue);
        sut.Next();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 00:00:00"));
        sut.Next();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("01/28/2023 00:00:00"));
        sut.Next();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("01/29/2023 00:00:00"));
        sut.Next();
        sut.Next();
        sut.Next();
        sut.Next();
        sut.Next();
        sut.Next();
        sut.Next();
        sut.Next();
        sut.Next();
        sut.Next();
        sut.Next();
        sut.Next();
        sut.Next();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("02/11/2023 00:00:00"));
        sut.Next();
        await Assert.That(sut.IsBeginningOfTime()).IsFalse();
        await Assert.That(sut.IsEndOfTime()).IsTrue();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("02/11/2023 00:00:00"));
        sut.Next();
        await Assert.That(sut.IsBeginningOfTime()).IsFalse();
        await Assert.That(sut.IsEndOfTime()).IsTrue();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("02/11/2023 00:00:00"));
        sut.Next();
        await Assert.That(sut.IsBeginningOfTime()).IsFalse();
        await Assert.That(sut.IsEndOfTime()).IsTrue();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("02/11/2023 00:00:00"));
        sut.Next();
        await Assert.That(sut.IsBeginningOfTime()).IsFalse();
        await Assert.That(sut.IsEndOfTime()).IsTrue();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("02/11/2023 00:00:00"));
        sut.Prior();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("02/10/2023 00:00:00"));
        await Assert.That(sut.IsBeginningOfTime()).IsFalse();
        await Assert.That(sut.IsEndOfTime()).IsFalse();
    }

    [Test]
    public async Task JumpByOneDayPriorHappyPath()
    {
        var sut = new StartOfDayTimeline<StartOfDayMoment>(new StartOfDayTimelineOptions(DateTime.MinValue));

        sut.Add(new StartOfDayMoment(DateTime.Parse("01/29/2023 00:00:00")));
        sut.Add(new StartOfDayMoment(DateTime.Parse("01/27/2023 00:00:00")));
        sut.MoveOnOrBefore(DateTime.Parse("01/29/2023 03:32:31 PM"));
        await Assert.That(sut.IsBeginningOfTime()).IsFalse();
        await Assert.That(sut.IsEndOfTime()).IsFalse();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("01/29/2023 00:00:00"));
        sut.Prior();
        await Assert.That(sut.IsBeginningOfTime()).IsFalse();
        await Assert.That(sut.IsEndOfTime()).IsFalse();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("01/28/2023 00:00:00"));
        sut.Prior();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 00:00:00"));
        sut.Prior();
        await Assert.That(sut.IsBeginningOfTime()).IsTrue();
        await Assert.That(sut.IsEndOfTime()).IsFalse();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.MinValue);
    }

    [Test]
    public async Task JumpAroundBeforeBeginningOfTime()
    {
        var sut = new StartOfDayTimeline<StartOfDayMoment>(new StartOfDayTimelineOptions(DateTime.MinValue));

        sut.Add(new StartOfDayMoment(DateTime.Parse("01/29/2023 00:00:00")));
        sut.Add(new StartOfDayMoment(DateTime.Parse("01/27/2023 00:00:00")));
        sut.MoveOnOrBefore(DateTime.Parse("01/27/2023 03:32:31 PM"));
        await Assert.That(sut.IsBeginningOfTime()).IsFalse();
        await Assert.That(sut.IsEndOfTime()).IsFalse();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 00:00:00"));
        sut.Prior();
        await Assert.That(sut.IsBeginningOfTime()).IsTrue();
        await Assert.That(sut.IsEndOfTime()).IsFalse();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.MinValue);
        sut.Prior();
        await Assert.That(sut.IsBeginningOfTime()).IsTrue();
        await Assert.That(sut.IsEndOfTime()).IsFalse();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.MinValue);
        sut.Next();
        await Assert.That(sut.IsBeginningOfTime()).IsFalse();
        await Assert.That(sut.IsEndOfTime()).IsFalse();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 00:00:00"));
    }

    [Test]
    public async Task TestMoveOnOrBefore()
    {
        var sut = new StartOfDayTimeline<StartOfDayMoment>(new StartOfDayTimelineOptions(DateTime.MinValue));

        sut.Add(new StartOfDayMoment(DateTime.Parse("01/29/2023 00:00:00")));
        sut.Add(new StartOfDayMoment(DateTime.Parse("01/27/2023 00:00:00")));

        //Between points
        sut.MoveOnOrBefore(DateTime.Parse("01/27/2023 03:32:31 PM"));
        await Assert.That(sut.IsBeginningOfTime()).IsFalse();
        await Assert.That(sut.IsEndOfTime()).IsFalse();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 00:00:00"));
        //On a point
        sut.MoveOnOrBefore(DateTime.Parse("01/27/2023"));
        await Assert.That(sut.IsBeginningOfTime()).IsFalse();
        await Assert.That(sut.IsEndOfTime()).IsFalse();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 00:00:00"));
        //Before First point
        sut.MoveOnOrBefore(DateTime.Parse("01/26/2023"));
        await Assert.That(sut.IsBeginningOfTime()).IsTrue();
        await Assert.That(sut.IsEndOfTime()).IsFalse();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.MinValue);
        //After last point
        sut.MoveOnOrBefore(DateTime.Parse("02/12/2023 13:00:00"));
        await Assert.That(sut.IsBeginningOfTime()).IsFalse();
        await Assert.That(sut.IsEndOfTime()).IsTrue();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("02/11/2023 00:00:00"));
    }

    [Test]
    public async Task TestMoveOnOrAfter()
    {
        var sut = new StartOfDayTimeline<StartOfDayMoment>(new StartOfDayTimelineOptions(DateTime.MinValue));

        sut.Add(new StartOfDayMoment(DateTime.Parse("01/29/2023 00:00:00")));
        sut.Add(new StartOfDayMoment(DateTime.Parse("01/27/2023 00:00:00")));

        //Between points
        sut.MoveOnOrAfter(DateTime.Parse("01/27/2023 03:32:31 PM"));
        await Assert.That(sut.IsBeginningOfTime()).IsFalse();
        await Assert.That(sut.IsEndOfTime()).IsFalse();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("01/28/2023 00:00:00"));
        //On a point
        sut.MoveOnOrAfter(DateTime.Parse("01/27/2023"));
        await Assert.That(sut.IsBeginningOfTime()).IsFalse();
        await Assert.That(sut.IsEndOfTime()).IsFalse();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 00:00:00"));
        //Before First point
        sut.MoveOnOrAfter(DateTime.Parse("01/26/2023"));
        await Assert.That(sut.IsBeginningOfTime()).IsFalse();
        await Assert.That(sut.IsEndOfTime()).IsFalse();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 00:00:00"));
        //After last point
        sut.MoveOnOrAfter(DateTime.Parse("02/12/2023 13:00:00"));
        await Assert.That(sut.IsBeginningOfTime()).IsFalse();
        await Assert.That(sut.IsEndOfTime()).IsTrue();
        await Assert.That(sut.CurrentMoment.Timestamp).IsEqualTo(DateTime.MaxValue);
    }

    [Test]
    public async Task BugAtEndOfTimeProof()
    {
        var navigator = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, true, 13));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 08:00:00"), DutyStatus.Sleeper));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 16:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 18:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 21:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 11:00:00"), DutyStatus.OffDuty));

        navigator.JumpTo(DateTime.Parse("8/24/2023 12:23 PM"));
        var endOfAuditWindow = navigator.FindRest(
            TimeSpan.FromHours(34),
            TimelineDirection.Forward,
            PreferredEndOfRest.Ending,
            MoveTo.None);
        var startOfAuditWindow = navigator.FindRest(
            TimeSpan.FromHours(34),
            TimelineDirection.Backward,
            PreferredEndOfRest.Beginning,
            MoveTo.NewLocation);

        await Assert.That(endOfAuditWindow.Timestamp).IsEqualTo(DateTime.Parse("09/07/2023"));
    }
}
