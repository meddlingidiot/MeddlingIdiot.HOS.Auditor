using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;
using MeddlingIdiot.HOS.TimelineNavigator.Segments;
using MeddlingIdiot.HOS.TimelineNavigator.Timelines;
using MeddlingIdiot.HOS.TimelineNavigator.Utilities;

namespace MeddlingIdiot.HOS.TimelineNavigator.UnitTests;

public class TimelineNavigatorTests
{
    [Test]
    public async Task Merge_timelines_together_for_MoveOnOrBefore()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions());
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/29/2023 08:00:00"), DutyStatus.Driving));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 01:00:00"), DutyStatus.Driving));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/28/2023 01:00:00"), DutyStatus.OnDuty));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 08:00:00"), DutyStatus.Sleeper));
        sut.Add(new GpsMoment(DateTime.Parse("01/27/2023 23:00:00"), (long)0.0, (long)0.0));
        sut.Add(new EngineBusMoment(DateTime.Parse("01/27/2023 23:00:00"), 1000));
        sut.Add(new GpsMoment(DateTime.Parse("01/27/2023 23:00:00"), (long)0.0, (long)0.0));
        sut.Add(new EngineBusMoment(DateTime.Parse("01/27/2023 23:00:00"), 1000));
        sut.Initialize();

        sut.JumpTo(DateTime.Parse("01/27/2023 00:30:00"));
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 00:00:00"));

        sut.JumpTo(DateTime.Parse("01/28/2023 00:00:00"));
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/28/2023 00:00:00"));

        sut.JumpTo(DateTime.Parse("01/27/2023 23:00:01"));
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 23:00:00"));
    }

    [Test]
    public async Task Merge_timelines_together_for_Next()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions());
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 01:00:00"), DutyStatus.Driving));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 08:00:00"), DutyStatus.Sleeper));
        sut.Add(new AnchorMoment(DateTime.Parse("01/27/2023 22:00:00"), "StartAudit"));
        sut.Add(new GpsMoment(DateTime.Parse("01/27/2023 23:00:00"), (long)0.0, (long)0.0));
        sut.Add(new EngineBusMoment(DateTime.Parse("01/27/2023 23:00:00"), 1000));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/28/2023 01:00:00"), DutyStatus.OnDuty));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/29/2023 08:00:00"), DutyStatus.Driving));
        sut.Initialize();

        sut.JumpTo(DateTime.Parse("01/27/2023 00:30:00"));
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 00:00:00"));
            await Assert.That(sut.Finish.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 01:00:00"));
            await Assert.That(sut.Length).IsEqualTo(TimeSpan.FromHours(1));
            await Assert.That(sut.IsStartOfDay).IsTrue();
        }
        sut.Next();
        using (Assert.Multiple())
        {
            await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 01:00:00"));
            await Assert.That(sut.Finish.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 08:00:00"));
            await Assert.That(sut.CurrentDutyStatusChangeMoment.CurrentDutyStatus).IsEqualTo(DutyStatus.Driving);
            await Assert.That(sut.DutyStatus).IsEqualTo(DutyStatus.Driving);
            await Assert.That(sut.Length).IsEqualTo(TimeSpan.FromHours(7));
            await Assert.That(sut.IsStartOfDay).IsFalse();
        }
        sut.Next();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 08:00:00"));
        sut.Next();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 22:00:00"));
        sut.Next();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 23:00:00"));
        await Assert.That(sut.Odometer).IsEqualTo(1000);
        sut.Next();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/28/2023 00:00:00"));
        sut.Next();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/28/2023 01:00:00"));
        sut.Next();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/29/2023 00:00:00"));
        sut.Next();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/29/2023 08:00:00"));
        sut.Next();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/30/2023 00:00:00"));
        sut.Next();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/31/2023 00:00:00"));
        sut.Next();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/01/2023 00:00:00"));
        sut.Next();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/02/2023 00:00:00"));
        sut.Next();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/03/2023 00:00:00"));
        sut.Next();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/04/2023 00:00:00"));
        sut.Next();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/05/2023 00:00:00"));
        sut.Next();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/06/2023 00:00:00"));
        sut.Next();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/07/2023 00:00:00"));
        sut.Next();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/08/2023 00:00:00"));
        sut.Next();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/09/2023 00:00:00"));
        sut.Next();
        await Assert.That(sut.IsEndOfTime()).IsFalse();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/10/2023 00:00:00"));
        sut.Next();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/11/2023 00:00:00"));
        sut.Next();
        using (Assert.Multiple())
        {
            await Assert.That(sut.IsEndOfTime()).IsTrue();
            await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/11/2023 00:00:00"));
            await Assert.That(sut.Finish.Timestamp).IsEqualTo(DateTime.MaxValue);
        }

        sut.Next();
    }

    [Test]
    public async Task Merge_timelines_together_for_Prior()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions());
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 01:00:00"), DutyStatus.Driving));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 08:00:00"), DutyStatus.Sleeper));
        sut.Add(new GpsMoment(DateTime.Parse("01/27/2023 23:00:00"), (long)0.0, (long)0.0));
        sut.Add(new EngineBusMoment(DateTime.Parse("01/27/2023 23:00:00"), 1000));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/28/2023 01:00:00"), DutyStatus.OnDuty));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/29/2023 08:00:00"), DutyStatus.Driving));
        sut.Initialize();

        sut.JumpTo(DateTime.Parse("02/13/2023 00:00:00"));
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/11/2023 00:00:00"));
        await Assert.That(sut.Finish.Timestamp).IsEqualTo(DateTime.MaxValue);
        await Assert.That(sut.Length).IsGreaterThan(TimeSpan.FromDays(999));
        await Assert.That(sut.IsEndOfTime()).IsTrue();
        await Assert.That(sut.IsStartOfDay).IsTrue();
        sut.Prior();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/10/2023 00:00:00"));
        await Assert.That(sut.Finish.Timestamp).IsEqualTo(DateTime.Parse("02/11/2023 00:00:00"));
        await Assert.That(sut.Length).IsEqualTo(TimeSpan.FromHours(24));
        await Assert.That(sut.IsEndOfTime()).IsFalse();
        sut.Prior();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/09/2023 00:00:00"));
        sut.Prior();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/08/2023 00:00:00"));
        sut.Prior();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/07/2023 00:00:00"));
        sut.Prior();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/06/2023 00:00:00"));
        sut.Prior();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/05/2023 00:00:00"));
        sut.Prior();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/04/2023 00:00:00"));
        sut.Prior();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/03/2023 00:00:00"));
        sut.Prior();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/02/2023 00:00:00"));
        sut.Prior();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("02/01/2023 00:00:00"));
        sut.Prior();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/31/2023 00:00:00"));
        sut.Prior();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/30/2023 00:00:00"));
        sut.Prior();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/29/2023 08:00:00"));
        sut.Prior();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/29/2023 00:00:00"));
        sut.Prior();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/28/2023 01:00:00"));
        sut.Prior();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/28/2023 00:00:00"));
        sut.Prior();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 23:00:00"));
        sut.Prior();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 08:00:00"));
        sut.Prior();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 01:00:00"));
        sut.Prior();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 00:00:00"));
        sut.Prior();
        await Assert.That(sut.IsBeginningOfTime()).IsTrue();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.MinValue);
        sut.Prior();
        await Assert.That(sut.IsBeginningOfTime()).IsTrue();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.MinValue);
    }

    [Test]
    public async Task Enumerate_through_all_moments_using_foreach()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions());
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 01:00:00"), DutyStatus.Driving));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 08:00:00"), DutyStatus.Sleeper));
        sut.Add(new AnchorMoment(DateTime.Parse("01/27/2023 22:00:00"), "StartAudit"));
        sut.Add(new GpsMoment(DateTime.Parse("01/27/2023 23:00:00"), (long)0.0, (long)0.0));
        sut.Add(new EngineBusMoment(DateTime.Parse("01/27/2023 23:00:00"), 1000));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/28/2023 01:00:00"), DutyStatus.OnDuty));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/29/2023 08:00:00"), DutyStatus.Driving));
        sut.Initialize();

        var moments = new List<Moment>();
        foreach (var moment in sut)
        {
            moments.Add(moment);
        }

        using (Assert.Multiple())
        {
            await Assert.That(moments.Count).IsEqualTo(23);
            await Assert.That(moments[0].Timestamp).IsEqualTo(DateTime.MinValue);
            await Assert.That(moments[1].Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 00:00:00"));
            await Assert.That(moments[2].Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 01:00:00"));
            await Assert.That(moments[3].Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 08:00:00"));
            await Assert.That(moments[4].Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 22:00:00"));
            await Assert.That(moments[5].Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 23:00:00"));
            await Assert.That(moments[6].Timestamp).IsEqualTo(DateTime.Parse("01/28/2023 00:00:00"));
            await Assert.That(moments[7].Timestamp).IsEqualTo(DateTime.Parse("01/28/2023 01:00:00"));
            await Assert.That(moments[20].Timestamp).IsEqualTo(DateTime.Parse("02/09/2023 00:00:00"));
            await Assert.That(moments[22].Timestamp).IsEqualTo(DateTime.Parse("02/11/2023 00:00:00"));
        }
    }

    [Test]
    public async Task Enumerate_through_empty_timeline()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Initialize();

        var moments = new List<Moment>();
        foreach (var moment in sut)
        {
            moments.Add(moment);
        }

        await Assert.That(moments.Count).IsEqualTo(1);
        await Assert.That(moments[0].Timestamp).IsEqualTo(DateTime.MinValue);
    }

    [Test]
    public async Task Track_agricultural_exception_enabled_state_using_segment()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions());
        sut.Upsert(new AgriculturalExceptionSegment(
            DateTime.Parse("01/27/2023 08:00:00"),
            DateTime.Parse("01/27/2023 16:00:00")));
        sut.Initialize();

        sut.JumpTo(DateTime.Parse("01/27/2023 07:59:59"));
        await Assert.That(sut.IsAgriculturalExceptionEnabled).IsFalse();

        sut.JumpTo(DateTime.Parse("01/27/2023 08:00:00"));
        await Assert.That(sut.IsAgriculturalExceptionEnabled).IsTrue();

        sut.JumpTo(DateTime.Parse("01/27/2023 12:00:00"));
        await Assert.That(sut.IsAgriculturalExceptionEnabled).IsTrue();

        sut.JumpTo(DateTime.Parse("01/27/2023 16:00:00"));
        await Assert.That(sut.IsAgriculturalExceptionEnabled).IsFalse();
    }

    [Test]
    public async Task Track_adverse_conditions_enabled_state_using_segment()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions());
        sut.Upsert(new AdverseConditionsSegment(
            DateTime.Parse("01/27/2023 10:00:00"),
            DateTime.Parse("01/27/2023 18:00:00")));
        sut.Initialize();

        sut.JumpTo(DateTime.Parse("01/27/2023 09:59:59"));
        await Assert.That(sut.IsAdverseConditionsEnabled).IsFalse();

        sut.JumpTo(DateTime.Parse("01/27/2023 10:00:00"));
        await Assert.That(sut.IsAdverseConditionsEnabled).IsTrue();

        sut.JumpTo(DateTime.Parse("01/27/2023 14:00:00"));
        await Assert.That(sut.IsAdverseConditionsEnabled).IsTrue();

        sut.JumpTo(DateTime.Parse("01/27/2023 18:00:00"));
        await Assert.That(sut.IsAdverseConditionsEnabled).IsFalse();
    }

    [Test]
    public async Task Track_event_moment_in_timeline_navigation()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 08:00:00"), DutyStatus.Driving));
        sut.Add(new EventMoment(DateTime.Parse("01/27/2023 10:00:00"), eventCode: 42, comment: "test event"));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 12:00:00"), DutyStatus.OnDuty));
        sut.Initialize();

        sut.JumpTo(DateTime.MinValue);
        sut.Next();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 08:00:00"));
        sut.Next();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 10:00:00"));
        sut.Next();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 12:00:00"));
    }

    [Test]
    public async Task Enumerate_and_access_moment_properties()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions());
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 01:00:00"), DutyStatus.Driving));
        sut.Add(new EngineBusMoment(DateTime.Parse("01/27/2023 02:00:00"), 1500));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 08:00:00"), DutyStatus.Sleeper));
        sut.Initialize();

        var dutyStatusMoments = new List<DutyStatusChangeMoment>();
        foreach (var moment in sut)
        {
            if (moment is DutyStatusChangeMoment dscMoment && dscMoment.Timestamp != DateTime.MinValue)
            {
                dutyStatusMoments.Add(dscMoment);
            }
        }

        using (Assert.Multiple())
        {
            await Assert.That(dutyStatusMoments.Count).IsEqualTo(2);
            await Assert.That(dutyStatusMoments[0].CurrentDutyStatus).IsEqualTo(DutyStatus.Driving);
            await Assert.That(dutyStatusMoments[1].CurrentDutyStatus).IsEqualTo(DutyStatus.Sleeper);
        }
    }

    [Test]
    public async Task Add_RestMoment_WithNormalTimestamp_AppearsInTimeline()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 08:00:00"), DutyStatus.OffDuty));
        sut.Add(new RestMoment(DateTime.Parse("01/27/2023 08:00:00"), TimeSpan.FromHours(10)));
        sut.Initialize();

        sut.JumpTo(DateTime.Parse("01/27/2023 08:00:00"));
        await Assert.That(sut.CurrentRestMoment.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 08:00:00"));
    }

    [Test]
    public async Task Add_RestMoment_WithMinValueTimestamp_RoutesToUpsert()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Add(new RestMoment(DateTime.MinValue, TimeSpan.FromHours(10)));
        sut.Initialize();

        sut.JumpTo(DateTime.MinValue);
        using (Assert.Multiple())
        {
            await Assert.That(sut.CurrentRestMoment.Timestamp).IsEqualTo(DateTime.MinValue);
            await Assert.That(sut.CurrentRestMoment.Duration).IsEqualTo(TimeSpan.FromHours(10));
        }
    }

    [Test]
    public async Task Add_EventMoment_WithMinValueTimestamp_RoutesToUpsert()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Add(new EventMoment(DateTime.MinValue, eventCode: 99));
        sut.Initialize();

        sut.JumpTo(DateTime.MinValue);
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.MinValue);
    }

    [Test]
    public async Task DefaultConstructor_CreatesNavigatorWithMidnightStartOfDay()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator();
        sut.Initialize();
        await Assert.That(sut.IsBeginningOfTime()).IsTrue();
    }

    [Test]
    public async Task Upsert_AnchorMoment_AppearsInTimeline()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Upsert(new AnchorMoment(DateTime.Parse("01/27/2023 08:00:00"), "audit"));
        sut.Initialize();

        sut.JumpTo(DateTime.Parse("01/27/2023 08:00:00"));
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 08:00:00"));
    }

    [Test]
    public async Task Upsert_DutyStatusChangeMoment_AppearsInTimeline()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Upsert(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 08:00:00"), DutyStatus.Driving));
        sut.Initialize();

        sut.JumpTo(DateTime.Parse("01/27/2023 08:00:00"));
        await Assert.That(sut.DutyStatus).IsEqualTo(DutyStatus.Driving);
    }

    [Test]
    public async Task Upsert_GpsMoment_ExposesLatitudeAndLongitude()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Upsert(new GpsMoment(DateTime.Parse("01/27/2023 08:00:00"), 45.5, -93.2));
        sut.Initialize();

        sut.JumpTo(DateTime.Parse("01/27/2023 08:00:00"));
        using (Assert.Multiple())
        {
            await Assert.That(sut.Latitude).IsEqualTo(45.5);
            await Assert.That(sut.Longitude).IsEqualTo(-93.2);
            await Assert.That(sut.CurrentGpsMoment.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 08:00:00"));
        }
    }

    [Test]
    public async Task Upsert_EngineBusMoment_ExposesOdometerAndCurrentMoment()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Upsert(new EngineBusMoment(DateTime.Parse("01/27/2023 08:00:00"), 5000));
        sut.Initialize();

        sut.JumpTo(DateTime.Parse("01/27/2023 08:00:00"));
        await Assert.That(sut.Odometer).IsEqualTo(5000);
        await Assert.That(sut.CurrentEngineBusMoment.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 08:00:00"));
    }

    [Test]
    public async Task Upsert_ShiftExtensionSegment_ExposesIsShiftExtended()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Upsert(new ShiftExtensionSegment(
            DateTime.Parse("01/27/2023 08:00:00"),
            DateTime.Parse("01/27/2023 16:00:00")));
        sut.Initialize();

        sut.JumpTo(DateTime.Parse("01/27/2023 08:00:00"));
        await Assert.That(sut.IsShiftExtended).IsTrue();

        sut.JumpTo(DateTime.Parse("01/27/2023 16:00:00"));
        await Assert.That(sut.IsShiftExtended).IsFalse();
    }

    [Test]
    public async Task IsEndOfSleeperSplits_ReturnsFalse_WhenRestMomentsExistAhead()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Add(new RestMoment(DateTime.Parse("01/27/2023 08:00:00"), TimeSpan.FromHours(10)));
        sut.Initialize();

        sut.JumpTo(DateTime.MinValue);
        await Assert.That(sut.IsEndOfSleeperSplits()).IsFalse();
    }

    [Test]
    public async Task IsEndOfShiftExtensions_ReturnsFalse_WhenShiftExtensionsExistAhead()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Upsert(new ShiftExtensionSegment(
            DateTime.Parse("01/27/2023 08:00:00"),
            DateTime.Parse("01/27/2023 16:00:00")));
        sut.Initialize();

        sut.JumpTo(DateTime.MinValue);
        await Assert.That(sut.IsEndOfShiftExtensions()).IsFalse();
    }

    [Test]
    public async Task StartOfDay_ReturnsCorrectMidnightForTimestamp()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions());
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 08:00:00"), DutyStatus.Driving));
        sut.Initialize();

        var result = sut.StartOfDay(DateTime.Parse("01/27/2023 14:00:00"));
        await Assert.That(result).IsEqualTo(DateTime.Parse("01/27/2023 00:00:00"));
    }

    [Test]
    public async Task CurrentDay_ReturnsStartOfDayForCurrentPosition()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions());
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 08:00:00"), DutyStatus.Driving));
        sut.Initialize();

        sut.JumpTo(DateTime.Parse("01/27/2023 10:00:00"));
        await Assert.That(sut.CurrentDay).IsEqualTo(DateTime.Parse("01/27/2023 00:00:00"));
    }

    [Test]
    public async Task StartAndFinishTimestamps_AreExposedAsDateTimeProperties()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 08:00:00"), DutyStatus.Driving));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 12:00:00"), DutyStatus.OnDuty));
        sut.Initialize();

        sut.JumpTo(DateTime.Parse("01/27/2023 08:00:00"));
        await Assert.That(sut.StartTimestamp).IsEqualTo(DateTime.Parse("01/27/2023 08:00:00"));
        await Assert.That(sut.FinishTimestamp).IsEqualTo(DateTime.Parse("01/27/2023 12:00:00"));
    }

    [Test]
    public async Task DriverIdNumber_And_TruckNumber_ArePopulatedFromMoment()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Upsert(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 08:00:00"), DutyStatus.Driving,
            driverIdNumber: "D123", truckNumber: "T456"));
        sut.Initialize();

        sut.JumpTo(DateTime.Parse("01/27/2023 08:00:00"));
        await Assert.That(sut.DriverIdNumber).IsEqualTo("D123");
        await Assert.That(sut.TruckNumber).IsEqualTo("T456");
    }

    [Test]
    public async Task DumpSplitRestTimeline_LogsEachRestMoment()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Add(new RestMoment(DateTime.Parse("01/27/2023 08:00:00"), TimeSpan.FromHours(10)));
        sut.Add(new RestMoment(DateTime.Parse("01/27/2023 20:00:00"), TimeSpan.FromHours(2)));
        sut.Initialize();

        var logger = new InMemoryLogger();
        logger.Initialize();
        sut.DumpSplitRestTimeline(logger);

        var log = logger.GetResults();
        await Assert.That(log).Contains("SplitRestMoment");
    }

    [Test]
    public async Task NonGenericGetEnumerator_IteratesAllMoments()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 08:00:00"), DutyStatus.Driving));
        sut.Initialize();

        var moments = new List<object>();
        System.Collections.IEnumerable nonGeneric = sut;
        foreach (var moment in nonGeneric)
        {
            moments.Add(moment);
        }

        await Assert.That(moments.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task JumpToPriorRest_WithNullPaired_MovesToPriorRest()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 08:00:00"), DutyStatus.OffDuty));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 20:00:00"), DutyStatus.Sleeper));
        sut.Add(new RestMoment(DateTime.Parse("01/27/2023 08:00:00"), TimeSpan.FromHours(10)));
        sut.Add(new RestMoment(DateTime.Parse("01/27/2023 20:00:00"), TimeSpan.FromHours(2)));
        sut.Initialize();

        sut.JumpToNextRest();
        sut.JumpToNextRest();
        sut.JumpToPriorRest();
        await Assert.That(sut.CurrentRestMoment.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 08:00:00"));
    }

    [Test]
    public async Task JumpToPriorRest_WithPairedFilter_MovesToMatchingRest()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 08:00:00"), DutyStatus.OffDuty));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 20:00:00"), DutyStatus.Sleeper));
        sut.Add(new RestMoment(DateTime.Parse("01/27/2023 08:00:00"), TimeSpan.FromHours(10), isPaired: false));
        sut.Add(new RestMoment(DateTime.Parse("01/27/2023 20:00:00"), TimeSpan.FromHours(2), isPaired: true));
        sut.Initialize();

        sut.JumpToNextRest();
        sut.JumpToNextRest();
        sut.JumpToPriorRest(paired: false);
        await Assert.That(sut.CurrentRestMoment.IsPaired).IsFalse();
    }

    [Test]
    public async Task JumpToNextRest_WithNullPaired_MovesToNextRest()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 08:00:00"), DutyStatus.OffDuty));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 20:00:00"), DutyStatus.Sleeper));
        sut.Add(new RestMoment(DateTime.Parse("01/27/2023 08:00:00"), TimeSpan.FromHours(10)));
        sut.Add(new RestMoment(DateTime.Parse("01/27/2023 20:00:00"), TimeSpan.FromHours(2)));
        sut.Initialize();

        sut.JumpToNextRest();
        await Assert.That(sut.CurrentRestMoment.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 08:00:00"));

        sut.JumpToNextRest();
        await Assert.That(sut.CurrentRestMoment.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 20:00:00"));
    }

    [Test]
    public async Task JumpToNextRest_WithPairedFilter_MovesToMatchingRest()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 08:00:00"), DutyStatus.OffDuty));
        sut.Add(new DutyStatusChangeMoment(DateTime.Parse("01/27/2023 20:00:00"), DutyStatus.Sleeper));
        sut.Add(new RestMoment(DateTime.Parse("01/27/2023 08:00:00"), TimeSpan.FromHours(10), isPaired: false));
        sut.Add(new RestMoment(DateTime.Parse("01/27/2023 20:00:00"), TimeSpan.FromHours(2), isPaired: true));
        sut.Initialize();

        sut.JumpToNextRest(paired: true);
        await Assert.That(sut.CurrentRestMoment.IsPaired).IsTrue();
    }

    [Test]
    public async Task JumpToPriorShiftExtension_WithNullFilter_MovesToPriorExtension()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Upsert(new ShiftExtensionSegment(
            DateTime.Parse("01/27/2023 08:00:00"),
            DateTime.Parse("01/27/2023 16:00:00")));
        sut.Initialize();

        sut.JumpToNextShiftExtension();
        sut.JumpToNextShiftExtension();
        sut.JumpToPriorShiftExtension();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 08:00:00"));
    }

    [Test]
    public async Task JumpToPriorShiftExtension_WithExtendedFilter_MovesToMatchingExtension()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Upsert(new ShiftExtensionSegment(
            DateTime.Parse("01/27/2023 08:00:00"),
            DateTime.Parse("01/27/2023 16:00:00"),
            isExtended: true));
        sut.Initialize();

        sut.JumpToNextShiftExtension();
        sut.JumpToNextShiftExtension();
        sut.JumpToPriorShiftExtension(isExtended: true);
        await Assert.That(sut.IsShiftExtended).IsTrue();
    }

    [Test]
    public async Task JumpToNextShiftExtension_WithNullFilter_MovesToNextExtension()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Upsert(new ShiftExtensionSegment(
            DateTime.Parse("01/27/2023 08:00:00"),
            DateTime.Parse("01/27/2023 16:00:00")));
        sut.Initialize();

        sut.JumpToNextShiftExtension();
        await Assert.That(sut.Start.Timestamp).IsEqualTo(DateTime.Parse("01/27/2023 08:00:00"));
    }

    [Test]
    public async Task JumpToNextShiftExtension_WithExtendedFilter_MovesToMatchingExtension()
    {
        var sut = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(null, false));
        sut.Upsert(new ShiftExtensionSegment(
            DateTime.Parse("01/27/2023 08:00:00"),
            DateTime.Parse("01/27/2023 16:00:00"),
            isExtended: true));
        sut.Initialize();

        sut.JumpToNextShiftExtension(isExtended: true);
        await Assert.That(sut.IsShiftExtended).IsTrue();
    }
}
