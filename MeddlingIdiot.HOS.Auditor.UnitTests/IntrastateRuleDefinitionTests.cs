using MeddlingIdiot.HOS;
using MeddlingIdiot.HOS.Queries;
using MeddlingIdiot.HOS.Ruleset;
using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;

namespace MeddlingIdiot.HOS.Auditor.UnitTests;

//State intrastate rulesets. Sources:
//  Texas      — 37 TAC §4.12: 12h driving after 8h off, no driving after 15h on
//               duty (cumulative), 70h/7 days, 34h restart, no 30-minute break.
//  California — 13 CCR §1212.5: 12h driving after 10h off, 16h window, 80h/8 days.
//  Florida    — Fla. Stat. §316.302: 12h driving after 10h off, 16h window,
//               70h/7 days (or 80h/8 for carriers operating every day), 34h restart.

public class IntrastateRuleDefinitionTests
{
    [Test]
    public async Task TexasIntrastate_HasTexasLimits()
    {
        var sut = new TexasIntrastate70HrRuleDefinition();

        using (Assert.Multiple())
        {
            await Assert.That(sut.MinDrivingLimit).IsEqualTo(TimeSpan.FromHours(12));
            await Assert.That(sut.MinFullRest).IsEqualTo(TimeSpan.FromHours(8));
            await Assert.That(sut.MinOnDutyLimit).IsEqualTo(TimeSpan.FromHours(15));
            await Assert.That(sut.MinShiftLimit).IsEqualTo(TimeSpan.Zero);           // cumulative 15h, not a window
            await Assert.That(sut.MaxUnbrokenDrivingLimit).IsEqualTo(TimeSpan.Zero); // no 30-min break in Texas
            await Assert.That(sut.NumberOfDaysInWindow).IsEqualTo(7);
            await Assert.That(sut.MinWindowLimit).IsEqualTo(TimeSpan.FromHours(70));
            await Assert.That(sut.GlobalReset).IsEqualTo(TimeSpan.FromHours(34));
            await Assert.That(sut.UsesPrimarySplit).IsFalse();
        }
    }

    [Test]
    public async Task CaliforniaIntrastate_HasCaliforniaLimits()
    {
        var sut = new CaliforniaIntrastate80HrRuleDefinition();

        using (Assert.Multiple())
        {
            await Assert.That(sut.MinDrivingLimit).IsEqualTo(TimeSpan.FromHours(12));
            await Assert.That(sut.MinFullRest).IsEqualTo(TimeSpan.FromHours(10));
            await Assert.That(sut.MinShiftLimit).IsEqualTo(TimeSpan.FromHours(16));  // wall-clock window
            await Assert.That(sut.MinOnDutyLimit).IsEqualTo(TimeSpan.Zero);
            await Assert.That(sut.MaxUnbrokenDrivingLimit).IsEqualTo(TimeSpan.Zero); // no 30-min break
            await Assert.That(sut.NumberOfDaysInWindow).IsEqualTo(8);
            await Assert.That(sut.MinWindowLimit).IsEqualTo(TimeSpan.FromHours(80));
            await Assert.That(sut.UsesPrimarySplit).IsFalse();
        }
    }

    [Test]
    public async Task FloridaIntrastate70_HasFloridaLimits()
    {
        var sut = new FloridaIntrastate70HrRuleDefinition();

        using (Assert.Multiple())
        {
            await Assert.That(sut.MinDrivingLimit).IsEqualTo(TimeSpan.FromHours(12));
            await Assert.That(sut.MinFullRest).IsEqualTo(TimeSpan.FromHours(10));
            await Assert.That(sut.MinShiftLimit).IsEqualTo(TimeSpan.FromHours(16));
            await Assert.That(sut.MinOnDutyLimit).IsEqualTo(TimeSpan.Zero);
            await Assert.That(sut.MaxUnbrokenDrivingLimit).IsEqualTo(TimeSpan.Zero);
            await Assert.That(sut.NumberOfDaysInWindow).IsEqualTo(7);
            await Assert.That(sut.MinWindowLimit).IsEqualTo(TimeSpan.FromHours(70));
            await Assert.That(sut.GlobalReset).IsEqualTo(TimeSpan.FromHours(34));
        }
    }

    [Test]
    public async Task FloridaIntrastate80_HasFloridaEverydayCarrierLimits()
    {
        var sut = new FloridaIntrastate80HrRuleDefinition();

        using (Assert.Multiple())
        {
            await Assert.That(sut.MinDrivingLimit).IsEqualTo(TimeSpan.FromHours(12));
            await Assert.That(sut.NumberOfDaysInWindow).IsEqualTo(8);
            await Assert.That(sut.MinWindowLimit).IsEqualTo(TimeSpan.FromHours(80));
        }
    }

    // ── Behavior: the state limits actually govern the audit ─────────────────

    //11.5h of driving after only 8h off — legal in Texas intrastate, multiple
    //violations under the federal rules. Proves the definition swaps the limits.
    [Test]
    public async Task TexasIntrastate_ElevenAndAHalfHoursDrivingAfterEightOff_NoViolations()
    {
        var navigator = BuildTexasDay();

        var sut = new HosAuditor(new TexasIntrastate70HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 AM"),
            DateTime.Parse("8/25/2023 12:25 AM"),
            navigator, AuditRules.AllRules));

        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task TexasIntrastate_ContrastCase_SameDayUnderFederalRules_WithViolations()
    {
        var navigator = BuildTexasDay();

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 AM"),
            DateTime.Parse("8/25/2023 12:25 AM"),
            navigator, AuditRules.AllRules));

        await Assert.That(violationResult.Violations.Count).IsGreaterThan(0);
    }

    //8h off duty (a full Texas rest, insufficient federally), then 11.5h driving
    //with no 30-minute break (no break rule in Texas).
    private static TimelineNavigator.TimelineNavigator BuildTexasDay()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));           //8h off
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 08:00:00"), DutyStatus.Driving));  //11.5h driving
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 19:30:00"), DutyStatus.OffDuty));
        return navigator;
    }

    //Florida: driving in hour 15 of the duty window is legal (16h window); the
    //same timeline violates the federal 14-hour window.
    [Test]
    public async Task FloridaIntrastate_DrivingInFifteenthHourOfWindow_NoViolations()
    {
        var navigator = BuildFloridaDay();

        var sut = new HosAuditor(new FloridaIntrastate70HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 AM"),
            DateTime.Parse("8/25/2023 12:25 AM"),
            navigator, AuditRules.AllRules));

        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task FloridaIntrastate_ContrastCase_SameDayUnderFederalRules_WithViolations()
    {
        var navigator = BuildFloridaDay();

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

    //Window opens 06:00. Federal 14th hour = 20:00; Florida 16th hour = 22:00.
    //The final driving hour 20:30–21:30 sits between the two, so it violates only
    //the federal window. Driving totals 9h with ≥30m breaks between stints, which
    //keeps the driving-limit and unbroken-driving rules quiet under both rulesets
    //— the contrast isolates the window rule.
    private static TimelineNavigator.TimelineNavigator BuildFloridaDay()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 06:00:00"), DutyStatus.OnDuty));   //window opens
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 07:00:00"), DutyStatus.Driving));  //4h
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 11:00:00"), DutyStatus.OffDuty));  //1h break
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 12:00:00"), DutyStatus.Driving));  //4h
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 16:00:00"), DutyStatus.OnDuty));   //4.5h working (also a break)
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 20:30:00"), DutyStatus.Driving));  //1h — past federal 14th hour, inside Florida's 16th
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 21:30:00"), DutyStatus.OffDuty));
        return navigator;
    }
}
