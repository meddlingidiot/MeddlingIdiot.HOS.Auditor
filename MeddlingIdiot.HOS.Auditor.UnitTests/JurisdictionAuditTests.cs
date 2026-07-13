using MeddlingIdiot.HOS;
using MeddlingIdiot.HOS.Queries;
using MeddlingIdiot.HOS.Ruleset;
using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;
using MeddlingIdiot.HOS.Violations;

namespace MeddlingIdiot.HOS.Auditor.UnitTests;

public class JurisdictionAuditTests
{
    //Base scenario from HosAuditorTests: under Us60Hr this produces an unbroken-driving
    //violation 8/25 05:00-09:00 (8h limit) and a driving violation 8/25 08:00-09:00 (11h limit).
    private static TimelineNavigator.TimelineNavigator BuildBaseNavigator(DateTime finalDrivingEnd)
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.Driving)); //8
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 08:00:00"), DutyStatus.Sleeper)); //8
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 16:00:00"), DutyStatus.OnDuty)); //2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 18:00:00"), DutyStatus.OffDuty)); //3
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 21:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(finalDrivingEnd, DutyStatus.OffDuty));
        return navigator;
    }

    private static ViolationResults Audit(TimelineNavigator.TimelineNavigator navigator)
    {
        var sut = new HosAuditor(new Us60HrRuleDefinition());
        return sut.AuditRange(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 PM"),
            DateTime.Parse("03/08/2024"),
            navigator, AuditRules.AllRules));
    }

    [Test]
    public async Task AuditRange_NullJurisdictionName_KeepsDefaultBehavior()
    {
        var navigator = BuildBaseNavigator(DateTime.Parse("8/25/2023 09:00:00"));
        navigator.Add(new JurisdictionMoment(DateTime.Parse("8/24/2023 12:00:00")));

        var violationResult = Audit(navigator);

        await Assert.That(violationResult.Violations.Count).IsEqualTo(2);
        await Assert.That(violationResult.Violations[0].Limit).IsEqualTo(TimeSpan.FromHours(8));
        await Assert.That(violationResult.Violations[0].StartTimestamp).IsEqualTo(DateTime.Parse("8/25/2023 05:00:00"));
        await Assert.That(violationResult.Violations[0].TimeInViolation).IsEqualTo(TimeSpan.FromHours(4));
        await Assert.That(violationResult.Violations[1].Limit).IsEqualTo(TimeSpan.FromHours(11));
        await Assert.That(violationResult.Violations[1].StartTimestamp).IsEqualTo(DateTime.Parse("8/25/2023 08:00:00"));
        await Assert.That(violationResult.Violations[1].TimeInViolation).IsEqualTo(TimeSpan.FromHours(1));
    }

    [Test]
    public async Task AuditRange_UnknownJurisdictionName_FallsBackToDefault()
    {
        var navigator = BuildBaseNavigator(DateTime.Parse("8/25/2023 09:00:00"));
        navigator.Add(new JurisdictionMoment(DateTime.Parse("8/24/2023 12:00:00"), "Narnia"));

        var violationResult = Audit(navigator);

        await Assert.That(violationResult.Violations.Count).IsEqualTo(2);
        await Assert.That(violationResult.Violations[0].Limit).IsEqualTo(TimeSpan.FromHours(8));
        await Assert.That(violationResult.Violations[0].StartTimestamp).IsEqualTo(DateTime.Parse("8/25/2023 05:00:00"));
        await Assert.That(violationResult.Violations[1].Limit).IsEqualTo(TimeSpan.FromHours(11));
        await Assert.That(violationResult.Violations[1].StartTimestamp).IsEqualTo(DateTime.Parse("8/25/2023 08:00:00"));
    }

    [Test]
    public async Task AuditRange_JurisdictionChange_SplitsViolationAtBoundaries()
    {
        //Texas is active 8/25 06:00-08:30, default (Us60Hr) before and after.
        //Us60Hr's unbroken-driving violation (05:00-09:00) is truncated at the Texas
        //border, and a new piece starts when the default jurisdiction resumes.
        //Texas itself throws nothing: the 8h sleeper is a full rest under Texas rules,
        //so the second driving stretch is exactly at its 12h limit.
        var navigator = BuildBaseNavigator(DateTime.Parse("8/25/2023 09:00:00"));
        navigator.Add(new JurisdictionMoment(DateTime.Parse("8/25/2023 06:00:00"), "TexasIntrastate70Hr"));
        navigator.Add(new JurisdictionMoment(DateTime.Parse("8/25/2023 08:30:00")));

        var violationResult = Audit(navigator);
        var violations = violationResult.Violations
            .OrderBy(v => v.StartTimestamp)
            .ThenBy(v => v.Limit)
            .ToList();

        await Assert.That(violations.Count).IsEqualTo(3);

        //Piece of the unbroken-driving violation before Texas
        await Assert.That(violations[0].Limit).IsEqualTo(TimeSpan.FromHours(8));
        await Assert.That(violations[0].StartTimestamp).IsEqualTo(DateTime.Parse("8/25/2023 05:00:00"));
        await Assert.That(violations[0].EndTimestamp).IsEqualTo(DateTime.Parse("8/25/2023 06:00:00"));
        await Assert.That(violations[0].TimeInViolation).IsEqualTo(TimeSpan.FromHours(1));

        //Piece of the unbroken-driving violation after Texas ends
        await Assert.That(violations[1].Limit).IsEqualTo(TimeSpan.FromHours(8));
        await Assert.That(violations[1].StartTimestamp).IsEqualTo(DateTime.Parse("8/25/2023 08:30:00"));
        await Assert.That(violations[1].EndTimestamp).IsEqualTo(DateTime.Parse("8/25/2023 09:00:00"));
        await Assert.That(violations[1].TimeInViolation).IsEqualTo(TimeSpan.FromMinutes(30));

        //Piece of the 11h driving violation (08:00-09:00) after Texas ends
        await Assert.That(violations[2].Limit).IsEqualTo(TimeSpan.FromHours(11));
        await Assert.That(violations[2].StartTimestamp).IsEqualTo(DateTime.Parse("8/25/2023 08:30:00"));
        await Assert.That(violations[2].EndTimestamp).IsEqualTo(DateTime.Parse("8/25/2023 09:00:00"));
        await Assert.That(violations[2].TimeInViolation).IsEqualTo(TimeSpan.FromMinutes(30));
    }

    [Test]
    public async Task AuditRange_SecondJurisdiction_ThrowsOnlyInsideItsWindow()
    {
        //Texas takes over at 8/25 06:00 and stays active to the end. Driving runs to
        //10:00 (13h stretch), so Texas's own 12h driving limit is broken at 09:00.
        //The default jurisdiction's violations are truncated at the Texas border.
        var navigator = BuildBaseNavigator(DateTime.Parse("8/25/2023 10:00:00"));
        navigator.Add(new JurisdictionMoment(DateTime.Parse("8/25/2023 06:00:00"), "TexasIntrastate70Hr"));

        var violationResult = Audit(navigator);
        var violations = violationResult.Violations
            .OrderBy(v => v.StartTimestamp)
            .ThenBy(v => v.Limit)
            .ToList();

        await Assert.That(violations.Count).IsEqualTo(2);

        //Default jurisdiction's unbroken-driving violation, truncated at the Texas border
        await Assert.That(violations[0].Limit).IsEqualTo(TimeSpan.FromHours(8));
        await Assert.That(violations[0].StartTimestamp).IsEqualTo(DateTime.Parse("8/25/2023 05:00:00"));
        await Assert.That(violations[0].EndTimestamp).IsEqualTo(DateTime.Parse("8/25/2023 06:00:00"));
        await Assert.That(violations[0].TimeInViolation).IsEqualTo(TimeSpan.FromHours(1));

        //Texas's 12h driving violation, thrown inside its own window
        await Assert.That(violations[1].Limit).IsEqualTo(TimeSpan.FromHours(12));
        await Assert.That(violations[1].StartTimestamp).IsEqualTo(DateTime.Parse("8/25/2023 09:00:00"));
        await Assert.That(violations[1].EndTimestamp).IsEqualTo(DateTime.Parse("8/25/2023 10:00:00"));
        await Assert.That(violations[1].TimeInViolation).IsEqualTo(TimeSpan.FromHours(1));
    }
}
