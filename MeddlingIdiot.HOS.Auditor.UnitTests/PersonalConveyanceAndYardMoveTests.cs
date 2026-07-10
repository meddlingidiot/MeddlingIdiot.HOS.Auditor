using MeddlingIdiot.HOS;
using MeddlingIdiot.HOS.Queries;
using MeddlingIdiot.HOS.Ruleset;
using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;

namespace MeddlingIdiot.HOS.Auditor.UnitTests;

//Personal Conveyance (PC) is the ELD "authorized personal use" special driving
//category: the vehicle moves while the driver is off duty. It must audit exactly
//like OffDuty. Yard Move (YM) is on-duty movement inside a yard: it must audit
//exactly like OnDuty — working time, but never driving time.
//Each scenario is paired with a contrast case proving the rule would fire if the
//special status were its "plain" driving/working counterpart.

public class PersonalConveyanceAndYardMoveTests
{
    [Test]
    public async Task PersonalConveyance_DoesNotAccumulateDrivingTime_NoViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.Driving));   //7h
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 17:00:00"), DutyStatus.OffDuty));   //30m break
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 17:30:00"), DutyStatus.Driving));   //3.5h -> 10.5h driving total
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 21:00:00"), DutyStatus.PersonalConveyance)); //2h moving, off duty
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 23:00:00"), DutyStatus.OffDuty));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 AM"),
            DateTime.Parse("8/25/2023 12:25 AM"),
            navigator, AuditRules.AllRules));

        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task PersonalConveyance_ContrastCase_SameMovementAsDriving_ViolatesDrivingLimit()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 17:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 17:30:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 21:00:00"), DutyStatus.Driving));   //the PC hours as real driving -> 12.5h
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 23:00:00"), DutyStatus.OffDuty));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 AM"),
            DateTime.Parse("8/25/2023 12:25 AM"),
            navigator, AuditRules.AllRules));

        using (Assert.Multiple())
        {
            await Assert.That(violationResult.Violations.Count).IsGreaterThan(0);
            await Assert.That(violationResult.Violations.Any(v => v.Comment.Contains("11"))).IsTrue();
        }
    }

    [Test]
    public async Task PersonalConveyance_DoesNotInterruptTenHourRest_NoViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 01:00:00"), DutyStatus.Driving));   //5h
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 06:00:00"), DutyStatus.OffDuty));   //4h
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.PersonalConveyance)); //2h PC inside the break
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 12:00:00"), DutyStatus.OffDuty));   //4h -> 10h continuous rest
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 16:00:00"), DutyStatus.Driving));   //7h on a fresh clock
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 23:00:00"), DutyStatus.OffDuty));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 AM"),
            DateTime.Parse("8/25/2023 12:25 AM"),
            navigator, AuditRules.AllRules));

        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task PersonalConveyance_ContrastCase_OnDutyInsteadBreaksTheRest_WithViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 01:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 06:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.OnDuty));    //working splits the rest in two
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 12:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 16:00:00"), DutyStatus.Driving));   //5h + 7h on the same clock
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 23:00:00"), DutyStatus.OffDuty));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 AM"),
            DateTime.Parse("8/25/2023 12:25 AM"),
            navigator, AuditRules.AllRules));

        await Assert.That(violationResult.Violations.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task YardMove_DoesNotAccumulateDrivingTime_AndSatisfiesThirtyMinuteBreak_NoViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.Driving));   //7h unbroken
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 17:00:00"), DutyStatus.YardMove));  //45m yard move = qualifying break
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 17:45:00"), DutyStatus.Driving));   //3.25h -> 10.25h driving total
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 21:00:00"), DutyStatus.OffDuty));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 AM"),
            DateTime.Parse("8/25/2023 12:25 AM"),
            navigator, AuditRules.AllRules));

        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task YardMove_ContrastCase_NoBreakBetweenDrivingSegments_ViolatesUnbrokenDriving()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.Driving));   //10.5h with no break
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 20:30:00"), DutyStatus.OffDuty));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 AM"),
            DateTime.Parse("8/25/2023 12:25 AM"),
            navigator, AuditRules.AllRules));

        using (Assert.Multiple())
        {
            await Assert.That(violationResult.Violations.Count).IsGreaterThan(0);
            await Assert.That(violationResult.Violations.Any(v => v.Comment.Contains("8"))).IsTrue();
        }
    }

    [Test]
    public async Task YardMove_AccumulatesShiftTime_DrivingAfterTheFourteenthHour_ViolatesShiftLimit()
    {
        //Driving past the 14th hour is only a violation if the yard moves counted
        //toward the shift: without them the shift holds just 10 driving hours.
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 06:00:00"), DutyStatus.YardMove));  //yard move starts the shift
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 07:00:00"), DutyStatus.Driving));   //4h
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 11:00:00"), DutyStatus.YardMove));  //1h (also a break)
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 12:00:00"), DutyStatus.Driving));   //5h -> 9h driving total
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 17:00:00"), DutyStatus.YardMove));  //3h -> shift hits 14h at 20:00
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 20:00:00"), DutyStatus.Driving));   //1h driving PAST the 14th hour
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 21:00:00"), DutyStatus.OffDuty));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 AM"),
            DateTime.Parse("8/25/2023 12:25 AM"),
            navigator, AuditRules.AllRules));

        using (Assert.Multiple())
        {
            await Assert.That(violationResult.Violations.Count).IsGreaterThan(0);
            await Assert.That(violationResult.Violations.Any(v => v.Comment.Contains("14"))).IsTrue();
        }
    }

    [Test]
    public async Task YardMove_CountsTowardWeeklyWindow_WithViolations()
    {
        //Six days of 9h on duty + 3h yard moves = 72h working, then driving on day
        //seven — driving past 60 hours in the 7-day window is the violation.
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        for (var day = 24; day <= 29; day++)
        {
            navigator.Add(new DutyStatusChangeMoment(DateTime.Parse($"8/{day}/2023"), DutyStatus.OffDuty));
            navigator.Add(new DutyStatusChangeMoment(DateTime.Parse($"8/{day}/2023 10:00:00"), DutyStatus.OnDuty));
            navigator.Add(new DutyStatusChangeMoment(DateTime.Parse($"8/{day}/2023 19:00:00"), DutyStatus.YardMove));
            navigator.Add(new DutyStatusChangeMoment(DateTime.Parse($"8/{day}/2023 22:00:00"), DutyStatus.OffDuty));
        }
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/30/2023 10:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/30/2023 11:00:00"), DutyStatus.OffDuty));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 AM"),
            DateTime.Parse("8/31/2023 12:25 AM"),
            navigator, AuditRules.AllRules));

        using (Assert.Multiple())
        {
            await Assert.That(violationResult.Violations.Count).IsGreaterThan(0);
            await Assert.That(violationResult.Violations.Any(v => v.Comment.Contains("60"))).IsTrue();
        }
    }

    [Test]
    public async Task PersonalConveyance_DoesNotCountTowardWeeklyWindow_NoViolations()
    {
        //Same week, but the 3 moving hours each day are personal conveyance: only
        //54h working, so the day-seven driving is legal.
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        for (var day = 24; day <= 29; day++)
        {
            navigator.Add(new DutyStatusChangeMoment(DateTime.Parse($"8/{day}/2023"), DutyStatus.OffDuty));
            navigator.Add(new DutyStatusChangeMoment(DateTime.Parse($"8/{day}/2023 10:00:00"), DutyStatus.OnDuty));
            navigator.Add(new DutyStatusChangeMoment(DateTime.Parse($"8/{day}/2023 19:00:00"), DutyStatus.PersonalConveyance));
            navigator.Add(new DutyStatusChangeMoment(DateTime.Parse($"8/{day}/2023 22:00:00"), DutyStatus.OffDuty));
        }
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/30/2023 10:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/30/2023 11:00:00"), DutyStatus.OffDuty));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 AM"),
            DateTime.Parse("8/31/2023 12:25 AM"),
            navigator, AuditRules.AllRules));

        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }
}
