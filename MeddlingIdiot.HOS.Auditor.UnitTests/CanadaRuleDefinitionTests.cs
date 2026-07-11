using MeddlingIdiot.HOS;
using MeddlingIdiot.HOS.Queries;
using MeddlingIdiot.HOS.Ruleset;
using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;

namespace MeddlingIdiot.HOS.Auditor.UnitTests;

// Canadian federal hours of service, south of latitude 60°N.
// Source: Commercial Vehicle Drivers Hours of Service Regulations, SOR/2005-313.
//   Shift (both cycles): 13h driving / 14h on duty before 8 consecutive hours off
//     (s.12–13); no driving after 16 elapsed hours (s.13(3)); no 30-minute break;
//     +2h adverse (s.76); sleeper split of two periods each ≥2h totalling ≥10h (s.18).
//   Cycle 1: 70h on duty in any 7 days (s.26), reset by 36h off (s.28(a)).
//   Cycle 2: 120h on duty in any 14 days (s.27(a)), reset by 72h off (s.28(b)).

public class CanadaRuleDefinitionTests
{
    // ── Parameters ───────────────────────────────────────────────────────────

    [Test]
    public async Task CanadaCycle1_HasCanadianLimits()
    {
        var sut = new CanadaCycle1RuleDefinition();

        using (Assert.Multiple())
        {
            await Assert.That(sut.MinDrivingLimit).IsEqualTo(TimeSpan.FromHours(13));   // s.12(1)/13(1)
            await Assert.That(sut.MinOnDutyLimit).IsEqualTo(TimeSpan.FromHours(14));    // s.12(2)/13(2)
            await Assert.That(sut.MinShiftLimit).IsEqualTo(TimeSpan.FromHours(16));     // s.13(3) elapsed window
            await Assert.That(sut.MinFullRest).IsEqualTo(TimeSpan.FromHours(8));        // s.13 consecutive rest
            await Assert.That(sut.MaxUnbrokenDrivingLimit).IsEqualTo(TimeSpan.Zero);    // no 30-minute break
            await Assert.That(sut.AdverseConditionsLimitExtension).IsEqualTo(TimeSpan.FromHours(2)); // s.76
            await Assert.That(sut.NumberOfDaysInWindow).IsEqualTo(7);                   // s.26
            await Assert.That(sut.MinWindowLimit).IsEqualTo(TimeSpan.FromHours(70));    // s.26
            await Assert.That(sut.GlobalReset).IsEqualTo(TimeSpan.FromHours(36));       // s.28(a)
            await Assert.That(sut.UsesPrimarySplit).IsFalse();                          // two-period symmetric split
            await Assert.That(sut.MinSplitRest).IsEqualTo(TimeSpan.FromHours(2));       // s.18 each period ≥2h
            await Assert.That(sut.MinSplitTotalRest).IsEqualTo(TimeSpan.FromHours(10)); // s.18 together ≥10h
            await Assert.That(sut.MinDailyOffDuty).IsEqualTo(TimeSpan.FromHours(10));          // s.14(1)
            await Assert.That(sut.MinDailyOffDutyBlockSize).IsEqualTo(TimeSpan.FromMinutes(30)); // s.14(2)
            await Assert.That(sut.MaxDailyOffDutyDeferral).IsEqualTo(TimeSpan.FromHours(2));   // s.16(1)(b)
            await Assert.That(sut.MaxTwoDayDrivingWithDeferral).IsEqualTo(TimeSpan.FromHours(26)); // s.16(1)(d)
            await Assert.That(sut.MinExtendedRest).IsEqualTo(TimeSpan.FromHours(24));   // s.25
            await Assert.That(sut.ExtendedRestLookbackDays).IsEqualTo(14);              // s.25
            await Assert.That(sut.MinOnDutyLimitWithoutExtendedRest).IsEqualTo(TimeSpan.Zero); // s.27(b) is Cycle 2 only
        }
    }

    [Test]
    public async Task CanadaCycle2_HasCanadianLimits()
    {
        var sut = new CanadaCycle2RuleDefinition();

        using (Assert.Multiple())
        {
            // Shift rules identical to Cycle 1.
            await Assert.That(sut.MinDrivingLimit).IsEqualTo(TimeSpan.FromHours(13));
            await Assert.That(sut.MinOnDutyLimit).IsEqualTo(TimeSpan.FromHours(14));
            await Assert.That(sut.MinShiftLimit).IsEqualTo(TimeSpan.FromHours(16));
            await Assert.That(sut.MinFullRest).IsEqualTo(TimeSpan.FromHours(8));
            await Assert.That(sut.MinSplitTotalRest).IsEqualTo(TimeSpan.FromHours(10));
            // Cycle differs.
            await Assert.That(sut.NumberOfDaysInWindow).IsEqualTo(14);                  // s.27(a)
            await Assert.That(sut.MinWindowLimit).IsEqualTo(TimeSpan.FromHours(120));   // s.27(a)
            await Assert.That(sut.GlobalReset).IsEqualTo(TimeSpan.FromHours(72));       // s.28(b)
            await Assert.That(sut.MinOnDutyLimitWithoutExtendedRest).IsEqualTo(TimeSpan.FromHours(70)); // s.27(b)
            await Assert.That(sut.MinExtendedRest).IsEqualTo(TimeSpan.FromHours(24));   // s.25 / s.27(b)
            await Assert.That(sut.ExtendedRestLookbackDays).IsEqualTo(14);              // s.25
            await Assert.That(sut.MinDailyOffDuty).IsEqualTo(TimeSpan.FromHours(10));   // s.14(1)
        }
    }

    // ── Behaviour: the Canadian limits actually govern the audit ──────────────

    // 12.5h of driving after only 8h off with no 30-minute break: legal in Canada
    // (13h driving after 8h off, no break rule), multiple violations under the US
    // rules (11h driving after 10h off, plus the 8h/30-min break requirement).
    [Test]
    public async Task Canada_TwelveAndAHalfHoursDrivingAfterEightOff_NoViolations()
    {
        var navigator = BuildLongDrivingDay();

        var sut = new HosAuditor(new CanadaCycle1RuleDefinition());
        var result = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 AM"),
            DateTime.Parse("8/25/2023 12:25 AM"),
            navigator, AuditRules.AllRules));

        await Assert.That(result.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Canada_ContrastCase_SameDayUnderUsRules_WithViolations()
    {
        var navigator = BuildLongDrivingDay();

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var result = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 AM"),
            DateTime.Parse("8/25/2023 12:25 AM"),
            navigator, AuditRules.AllRules));

        await Assert.That(result.Violations.Count).IsGreaterThan(0);
    }

    // 8h off (a full Canadian rest), then 12.5h of straight driving.
    private static TimelineNavigator.TimelineNavigator BuildLongDrivingDay()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));          // 8h off
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 08:00:00"), DutyStatus.Driving)); // 12.5h driving
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 20:30:00"), DutyStatus.OffDuty));
        return navigator;
    }

    // Driving in the 15th hour of the duty window is legal in Canada (16h window,
    // s.13(3)); the same timeline violates the US 14-hour window.
    [Test]
    public async Task Canada_DrivingInFifteenthHourOfWindow_NoViolations()
    {
        var navigator = BuildWideWindowDay();

        var sut = new HosAuditor(new CanadaCycle1RuleDefinition());
        var result = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 AM"),
            DateTime.Parse("8/25/2023 12:25 AM"),
            navigator, AuditRules.AllRules));

        await Assert.That(result.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Canada_ContrastCase_FifteenthHourUnderUsRules_WithViolations()
    {
        var navigator = BuildWideWindowDay();

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var result = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 AM"),
            DateTime.Parse("8/25/2023 12:25 AM"),
            navigator, AuditRules.AllRules));

        using (Assert.Multiple())
        {
            await Assert.That(result.Violations.Count).IsGreaterThan(0);
            await Assert.That(result.Violations.Any(v => v.Comment.Contains("14"))).IsTrue();
        }
    }

    // Window opens 06:00. US 14th hour = 20:00; Canada 16th hour = 22:00. The final
    // driving stint 20:00–21:00 crosses the US mark but stays inside Canada's window.
    // The mid-shift break is 1.5h — under the 2-hour split minimum, so it does not
    // pause either window — and on-duty filler keeps worked time at 13.5h (< 14) so
    // only the elapsed-window rule separates the two rulesets.
    private static TimelineNavigator.TimelineNavigator BuildWideWindowDay()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 06:00:00"), DutyStatus.OnDuty));   // window opens
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 07:00:00"), DutyStatus.Driving));  // 4h
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 11:00:00"), DutyStatus.OffDuty));  // 1.5h break
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 12:30:00"), DutyStatus.Driving));  // 4h
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 16:30:00"), DutyStatus.OnDuty));   // 3.5h
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 20:00:00"), DutyStatus.Driving));  // 1h — hour 14 of the window
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 21:00:00"), DutyStatus.OffDuty));
        return navigator;
    }

    // Accumulate 14.5h of on-duty time (4h driving + 10.5h on duty) then drive: the
    // driving after the 14-hour on-duty limit (s.12(2)/13(2)) is the violation. Driving
    // totals only 4.5h (< 13) and the shift fits inside the 16-hour window, so the
    // on-duty limit is what fires.
    [Test]
    public async Task Canada_DrivingAfterFourteenHoursOnDuty_ThrowsOnDutyViolation()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));          // 8h off
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 08:00:00"), DutyStatus.Driving)); // 4h driving
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 12:00:00"), DutyStatus.OnDuty));  // 10.5h on duty -> 14.5h worked
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 22:30:00"), DutyStatus.Driving)); // drive while over 14h
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 23:00:00"), DutyStatus.OffDuty));

        var sut = new HosAuditor(new CanadaCycle1RuleDefinition());
        var result = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 AM"),
            DateTime.Parse("8/25/2023 12:25 AM"),
            navigator, AuditRules.AllRules));

        using (Assert.Multiple())
        {
            await Assert.That(result.Violations.Count).IsGreaterThan(0);
            await Assert.That(result.Violations.Any(v => v.Limit == TimeSpan.FromHours(14))).IsTrue();
        }
    }
}
