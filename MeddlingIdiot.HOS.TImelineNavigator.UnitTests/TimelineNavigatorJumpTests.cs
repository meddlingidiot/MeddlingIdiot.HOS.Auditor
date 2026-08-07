using MeddlingIdiot.HOS.TimelineNavigator.Moments;
using MeddlingIdiot.HOS.TimelineNavigator.Segments;
using MeddlingIdiot.HOS.TimelineNavigator.Timelines;

namespace MeddlingIdiot.HOS.TimelineNavigator.UnitTests;

/// <summary>
/// Covers JumpToNextRest / JumpToPriorRest / JumpToNextShiftExtension / JumpToPriorShiftExtension.
/// The rest and shift extension timelines carry their own cursor, so these tests walk the cursor
/// all the way to both ends and exercise the optional filter (matching, skipping and no match at all).
/// </summary>
public class TimelineNavigatorJumpTests
{
    private static readonly DateTime FirstRest = DateTime.Parse("01/27/2023 08:00:00");
    private static readonly DateTime SecondRest = DateTime.Parse("01/27/2023 22:00:00");
    private static readonly DateTime ThirdRest = DateTime.Parse("01/28/2023 04:00:00");

    private static readonly DateTime FirstExtensionStart = DateTime.Parse("01/27/2023 08:00:00");
    private static readonly DateTime FirstExtensionFinish = DateTime.Parse("01/27/2023 16:00:00");
    private static readonly DateTime SecondExtensionStart = DateTime.Parse("01/28/2023 08:00:00");
    private static readonly DateTime SecondExtensionFinish = DateTime.Parse("01/28/2023 16:00:00");

    /// <summary>
    /// Three rests, each one landing on a duty status change so a jump can settle exactly on the rest.
    /// </summary>
    private static MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator BuildRestNavigator(
        bool firstIsPaired = false,
        bool secondIsPaired = true,
        bool thirdIsPaired = true)
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Add(new DutyStatusChangeMoment(FirstRest, DutyStatus.OffDuty));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 18:00:00"), DutyStatus.OnDuty));
        sut.Add(new DutyStatusChangeMoment(SecondRest, DutyStatus.Sleeper));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/28/2023 00:00:00"), DutyStatus.OnDuty));
        sut.Add(new DutyStatusChangeMoment(ThirdRest, DutyStatus.Sleeper));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/28/2023 12:00:00"), DutyStatus.OnDuty));

        sut.Add(new RestMoment(FirstRest, FirstRest, TimeSpan.FromHours(10), isPaired: firstIsPaired));
        sut.Add(new RestMoment(SecondRest, SecondRest, TimeSpan.FromHours(2), isPaired: secondIsPaired));
        sut.Add(new RestMoment(ThirdRest, ThirdRest, TimeSpan.FromHours(8), isPaired: thirdIsPaired));
        sut.Initialize();
        return sut;
    }

    /// <summary>
    /// Two extended segments, which produce four shift extension moments: on, off, on, off.
    /// </summary>
    private static MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator BuildShiftExtensionNavigator()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Upsert(new ShiftExtensionSegment(FirstExtensionStart, FirstExtensionFinish, isExtended: true));
        sut.Upsert(new ShiftExtensionSegment(SecondExtensionStart, SecondExtensionFinish, isExtended: true));
        sut.Initialize();
        return sut;
    }

    [Test]
    public async Task JumpToNextRest_WithoutFilter_MovesToEachRestInOrder()
    {
        var sut = BuildRestNavigator();

        sut.JumpToNextRest();
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(FirstRest);
            await Assert.That(sut.CurrentRestMoment.Timestamp).IsEqualTo(FirstRest);
            await Assert.That(sut.CurrentRestMoment.Duration).IsEqualTo(TimeSpan.FromHours(10));
            await Assert.That(sut.DutyStatus).IsEqualTo(DutyStatus.OffDuty);
        }

        sut.JumpToNextRest();
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(SecondRest);
            await Assert.That(sut.CurrentRestMoment.Timestamp).IsEqualTo(SecondRest);
            await Assert.That(sut.CurrentRestMoment.Duration).IsEqualTo(TimeSpan.FromHours(2));
            await Assert.That(sut.DutyStatus).IsEqualTo(DutyStatus.Sleeper);
        }

        sut.JumpToNextRest();
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(ThirdRest);
            await Assert.That(sut.CurrentRestMoment.Timestamp).IsEqualTo(ThirdRest);
            await Assert.That(sut.CurrentRestMoment.Duration).IsEqualTo(TimeSpan.FromHours(8));
        }
    }

    [Test]
    public async Task JumpToNextRest_WithoutFilter_StopsOnLastRest()
    {
        var sut = BuildRestNavigator();

        sut.JumpToNextRest();
        sut.JumpToNextRest();
        sut.JumpToNextRest();
        await Assert.That(sut.IsEndOfSleeperSplits()).IsTrue();

        sut.JumpToNextRest();
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(ThirdRest);
            await Assert.That(sut.CurrentRestMoment.Timestamp).IsEqualTo(ThirdRest);
            await Assert.That(sut.IsEndOfSleeperSplits()).IsTrue();
        }
    }

    [Test]
    public async Task JumpToNextRest_WithPairedFilter_SkipsRestsThatDoNotMatch()
    {
        var sut = BuildRestNavigator(firstIsPaired: false, secondIsPaired: true, thirdIsPaired: true);

        sut.JumpToNextRest(paired: true);
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(SecondRest);
            await Assert.That(sut.CurrentRestMoment.Timestamp).IsEqualTo(SecondRest);
            await Assert.That(sut.CurrentRestMoment.IsPaired).IsTrue();
        }

        sut.JumpToNextRest(paired: true);
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(ThirdRest);
            await Assert.That(sut.CurrentRestMoment.IsPaired).IsTrue();
        }
    }

    [Test]
    public async Task JumpToNextRest_WithUnpairedFilter_MovesToUnpairedRest()
    {
        var sut = BuildRestNavigator(firstIsPaired: false, secondIsPaired: true, thirdIsPaired: true);

        sut.JumpToNextRest(paired: false);
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(FirstRest);
            await Assert.That(sut.CurrentRestMoment.IsPaired).IsFalse();
        }
    }

    [Test]
    public async Task JumpToNextRest_WithFilterThatNeverMatches_StopsAtEndOfRestTimeline()
    {
        var sut = BuildRestNavigator(firstIsPaired: false, secondIsPaired: false, thirdIsPaired: false);

        sut.JumpToNextRest(paired: true);
        using (Assert.Multiple())
        {
            await Assert.That(sut.IsEndOfSleeperSplits()).IsTrue();
            await Assert.That(sut.Start.Timestamp).IsEqualTo(ThirdRest);
            await Assert.That(sut.CurrentRestMoment.IsPaired).IsFalse();
        }
    }

    [Test]
    public async Task JumpToNextRest_WithNoRestMoments_StaysAtBeginningOfTime()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Add(new DutyStatusChangeMoment(FirstRest, DutyStatus.Driving));
        sut.Initialize();

        sut.JumpToNextRest();
        using (Assert.Multiple())
        {
            await Assert.That(sut.IsBeginningOfTime()).IsTrue();
            await Assert.That(sut.CurrentRestMoment.Timestamp).IsEqualTo(DateTime.MinValue);
        }
    }

    [Test]
    public async Task JumpToPriorRest_WithoutFilter_MovesBackThroughEachRest()
    {
        var sut = BuildRestNavigator();
        sut.JumpToNextRest();
        sut.JumpToNextRest();
        sut.JumpToNextRest();

        sut.JumpToPriorRest();
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(SecondRest);
            await Assert.That(sut.CurrentRestMoment.Timestamp).IsEqualTo(SecondRest);
            await Assert.That(sut.DutyStatus).IsEqualTo(DutyStatus.Sleeper);
        }

        sut.JumpToPriorRest();
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(FirstRest);
            await Assert.That(sut.CurrentRestMoment.Timestamp).IsEqualTo(FirstRest);
            await Assert.That(sut.DutyStatus).IsEqualTo(DutyStatus.OffDuty);
        }
    }

    [Test]
    public async Task JumpToPriorRest_WithoutFilter_StopsAtBeginningOfTime()
    {
        var sut = BuildRestNavigator();
        sut.JumpToNextRest();

        sut.JumpToPriorRest();
        using (Assert.Multiple())
        {
            await Assert.That(sut.IsBeginningOfTime()).IsTrue();
            await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.MinValue);
            await Assert.That(sut.CurrentRestMoment.Timestamp).IsEqualTo(DateTime.MinValue);
        }

        sut.JumpToPriorRest();
        using (Assert.Multiple())
        {
            await Assert.That(sut.IsBeginningOfTime()).IsTrue();
            await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.MinValue);
        }
    }

    [Test]
    public async Task JumpToPriorRest_WithPairedFilter_SkipsRestsThatDoNotMatch()
    {
        var sut = BuildRestNavigator(firstIsPaired: true, secondIsPaired: false, thirdIsPaired: false);
        sut.JumpToNextRest();
        sut.JumpToNextRest();
        sut.JumpToNextRest();

        sut.JumpToPriorRest(paired: true);
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(FirstRest);
            await Assert.That(sut.CurrentRestMoment.Timestamp).IsEqualTo(FirstRest);
            await Assert.That(sut.CurrentRestMoment.IsPaired).IsTrue();
        }
    }

    [Test]
    public async Task JumpToPriorRest_WithFilterThatNeverMatches_StopsAtBeginningOfTime()
    {
        var sut = BuildRestNavigator(firstIsPaired: false, secondIsPaired: false, thirdIsPaired: false);
        sut.JumpToNextRest();
        sut.JumpToNextRest();
        sut.JumpToNextRest();

        sut.JumpToPriorRest(paired: true);
        using (Assert.Multiple())
        {
            await Assert.That(sut.IsBeginningOfTime()).IsTrue();
            await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.MinValue);
        }
    }

    [Test]
    public async Task JumpToNextShiftExtension_WithoutFilter_MovesToEachExtensionBoundary()
    {
        var sut = BuildShiftExtensionNavigator();

        sut.JumpToNextShiftExtension();
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(FirstExtensionStart);
            await Assert.That(sut.IsShiftExtended).IsTrue();
        }

        sut.JumpToNextShiftExtension();
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(FirstExtensionFinish);
            await Assert.That(sut.IsShiftExtended).IsFalse();
        }

        sut.JumpToNextShiftExtension();
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(SecondExtensionStart);
            await Assert.That(sut.IsShiftExtended).IsTrue();
        }

        sut.JumpToNextShiftExtension();
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(SecondExtensionFinish);
            await Assert.That(sut.IsShiftExtended).IsFalse();
            await Assert.That(sut.IsEndOfShiftExtensions()).IsTrue();
        }
    }

    [Test]
    public async Task JumpToNextShiftExtension_WithoutFilter_StopsOnLastExtensionBoundary()
    {
        var sut = BuildShiftExtensionNavigator();
        sut.JumpToNextShiftExtension();
        sut.JumpToNextShiftExtension();
        sut.JumpToNextShiftExtension();
        sut.JumpToNextShiftExtension();

        sut.JumpToNextShiftExtension();
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(SecondExtensionFinish);
            await Assert.That(sut.IsEndOfShiftExtensions()).IsTrue();
        }
    }

    [Test]
    public async Task JumpToNextShiftExtension_WithExtendedFilter_SkipsMomentsThatDoNotMatch()
    {
        var sut = BuildShiftExtensionNavigator();

        sut.JumpToNextShiftExtension(isExtended: true);
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(FirstExtensionStart);
            await Assert.That(sut.IsShiftExtended).IsTrue();
        }

        sut.JumpToNextShiftExtension(isExtended: true);
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(SecondExtensionStart);
            await Assert.That(sut.IsShiftExtended).IsTrue();
        }
    }

    [Test]
    public async Task JumpToNextShiftExtension_WithNotExtendedFilter_MovesToEndOfExtension()
    {
        var sut = BuildShiftExtensionNavigator();

        sut.JumpToNextShiftExtension(isExtended: false);
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(FirstExtensionFinish);
            await Assert.That(sut.IsShiftExtended).IsFalse();
        }
    }

    [Test]
    public async Task JumpToNextShiftExtension_WithNoShiftExtensions_StaysAtBeginningOfTime()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Add(new DutyStatusChangeMoment(FirstExtensionStart, DutyStatus.Driving));
        sut.Initialize();

        sut.JumpToNextShiftExtension();
        using (Assert.Multiple())
        {
            await Assert.That(sut.IsBeginningOfTime()).IsTrue();
            await Assert.That(sut.IsShiftExtended).IsFalse();
        }
    }

    [Test]
    public async Task JumpToPriorShiftExtension_WithoutFilter_MovesBackThroughEachExtensionBoundary()
    {
        var sut = BuildShiftExtensionNavigator();
        sut.JumpToNextShiftExtension();
        sut.JumpToNextShiftExtension();
        sut.JumpToNextShiftExtension();
        sut.JumpToNextShiftExtension();

        sut.JumpToPriorShiftExtension();
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(SecondExtensionStart);
            await Assert.That(sut.IsShiftExtended).IsTrue();
        }

        sut.JumpToPriorShiftExtension();
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(FirstExtensionFinish);
            await Assert.That(sut.IsShiftExtended).IsFalse();
        }

        sut.JumpToPriorShiftExtension();
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(FirstExtensionStart);
            await Assert.That(sut.IsShiftExtended).IsTrue();
        }
    }

    [Test]
    public async Task JumpToPriorShiftExtension_WithoutFilter_StopsAtBeginningOfTime()
    {
        var sut = BuildShiftExtensionNavigator();
        sut.JumpToNextShiftExtension();

        sut.JumpToPriorShiftExtension();
        using (Assert.Multiple())
        {
            await Assert.That(sut.IsBeginningOfTime()).IsTrue();
            await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.MinValue);
            await Assert.That(sut.IsShiftExtended).IsFalse();
        }

        sut.JumpToPriorShiftExtension();
        using (Assert.Multiple())
        {
            await Assert.That(sut.IsBeginningOfTime()).IsTrue();
            await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.MinValue);
        }
    }

    [Test]
    public async Task JumpToPriorShiftExtension_WithExtendedFilter_SkipsMomentsThatDoNotMatch()
    {
        var sut = BuildShiftExtensionNavigator();
        sut.JumpToNextShiftExtension();
        sut.JumpToNextShiftExtension();
        sut.JumpToNextShiftExtension();
        sut.JumpToNextShiftExtension();

        sut.JumpToPriorShiftExtension(isExtended: true);
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(SecondExtensionStart);
            await Assert.That(sut.IsShiftExtended).IsTrue();
        }

        sut.JumpToPriorShiftExtension(isExtended: true);
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(FirstExtensionStart);
            await Assert.That(sut.IsShiftExtended).IsTrue();
        }
    }

    [Test]
    public async Task JumpToPriorShiftExtension_WithFilterThatNeverMatches_StopsAtBeginningOfTime()
    {
        var sut = BuildShiftExtensionNavigator();
        sut.JumpToNextShiftExtension();
        sut.JumpToNextShiftExtension();

        // The cursor sits on the end of the first extension. The first call back finds the start of
        // that extension; the second has nothing extended left behind it and runs off the front.
        sut.JumpToPriorShiftExtension(isExtended: true);
        sut.JumpToPriorShiftExtension(isExtended: true);
        using (Assert.Multiple())
        {
            await Assert.That(sut.IsBeginningOfTime()).IsTrue();
            await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.MinValue);
        }
    }
}
