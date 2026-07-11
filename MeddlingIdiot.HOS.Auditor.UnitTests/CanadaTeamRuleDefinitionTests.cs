using MeddlingIdiot.HOS;
using MeddlingIdiot.HOS.Queries;
using MeddlingIdiot.HOS.Ruleset;
using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;

namespace MeddlingIdiot.HOS.Auditor.UnitTests;

// Canadian team sleeper splits (SOR/2005-313 s.19): a team of drivers may split
// off-duty time into two sleeper periods, each ≥ 4 hours, together ≥ 8 hours —
// versus the single-driver s.18 split of ≥ 2 hours each totalling ≥ 10. Under the
// s.19 chapeau the split satisfies both s.13 (consecutive rest) and s.14 (the
// daily minimum), so a paired-split day only needs the 8-hour split total.
public class CanadaTeamRuleDefinitionTests
{
    private static TimelineNavigator.TimelineNavigator Build(params (string Timestamp, DutyStatus DutyStatus)[] points)
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        foreach (var (timestamp, dutyStatus) in points)
            navigator.Add(new DutyStatusChangeMoment(DateTime.Parse(timestamp), dutyStatus));
        return navigator;
    }

    [Test]
    public async Task TeamDefinitions_UseTheTeamSplitAndMatchTheirSoloCounterpartsOtherwise()
    {
        var teamCycle1 = new CanadaTeamCycle1RuleDefinition();
        var teamCycle2 = new CanadaTeamCycle2RuleDefinition();
        var teamAuto = new CanadaTeamAutoCycleRuleDefinition();

        using (Assert.Multiple())
        {
            foreach (IRuleDefinition team in new IRuleDefinition[] { teamCycle1, teamCycle2, teamAuto })
            {
                await Assert.That(team.MinSplitRest).IsEqualTo(TimeSpan.FromHours(4));      // s.19: each ≥ 4h
                await Assert.That(team.MinSplitTotalRest).IsEqualTo(TimeSpan.FromHours(8)); // s.19: together ≥ 8h
                await Assert.That(team.MinDrivingLimit).IsEqualTo(TimeSpan.FromHours(13));
                await Assert.That(team.MinOnDutyLimit).IsEqualTo(TimeSpan.FromHours(14));
                await Assert.That(team.MinDailyOffDuty).IsEqualTo(TimeSpan.FromHours(10));
            }
            await Assert.That(teamCycle1.GlobalReset).IsEqualTo(TimeSpan.FromHours(36));
            await Assert.That(teamCycle2.GlobalReset).IsEqualTo(TimeSpan.FromHours(72));
            await Assert.That(teamCycle2.MinOnDutyLimitWithoutExtendedRest).IsEqualTo(TimeSpan.FromHours(70));
            await Assert.That(teamAuto.Cycle1WindowLimit).IsEqualTo(TimeSpan.FromHours(70));
            await Assert.That(teamAuto.Cycle2WindowLimit).IsEqualTo(TimeSpan.FromHours(120));
            await Assert.That(((IRuleDefinition)teamAuto).WindowRuleThrowsViolations).IsFalse();
        }
    }

    // ── The 4h + 4h split pairs for a team, not for a single driver ──────────
    //
    // 6h driving, a 4h sleeper period, 6h more driving, a second 4h sleeper
    // period, then 4h more driving (16h total since the last full rest). For a
    // team the periods pair (4 ≥ 4, 8 ≥ 8), the rules reset at the first period,
    // and no stretch exceeds the limits. For a single driver 4 + 4 = 8h is under
    // the 10-hour s.18 total, nothing pairs, and the accumulated 16h of driving
    // crosses the 13-hour limit at 05:00 on day two.
    private static TimelineNavigator.TimelineNavigator BuildTeamSplitDay() => Build(
        ("1/01/2024 00:00", DutyStatus.OffDuty),   // 8h full rest
        ("1/01/2024 08:00", DutyStatus.Driving),   // 6h
        ("1/01/2024 14:00", DutyStatus.Sleeper),   // 4h — period 1
        ("1/01/2024 18:00", DutyStatus.Driving),   // 6h
        ("1/02/2024 00:00", DutyStatus.Sleeper),   // 4h — period 2
        ("1/02/2024 04:00", DutyStatus.Driving),   // 4h
        ("1/02/2024 08:00", DutyStatus.OffDuty),
        ("1/03/2024 08:00", DutyStatus.Unknown));

    [Test]
    public async Task FourPlusFourSplit_TeamRuleset_NoViolations()
    {
        var sut = new HosAuditor(new CanadaTeamCycle1RuleDefinition());
        var result = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("1/01/2024 01:00"),
            DateTime.Parse("1/02/2024 12:00"),
            BuildTeamSplitDay(), AuditRules.AllRules));

        await Assert.That(result.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task FourPlusFourSplit_SoloRuleset_ThrowsDrivingViolation()
    {
        var sut = new HosAuditor(new CanadaCycle1RuleDefinition());
        var result = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("1/01/2024 01:00"),
            DateTime.Parse("1/02/2024 12:00"),
            BuildTeamSplitDay(), AuditRules.AllRules));

        using (Assert.Multiple())
        {
            await Assert.That(result.Violations.Count).IsGreaterThan(0);
            await Assert.That(result.Violations.Any(v => v.Limit == TimeSpan.FromHours(13))).IsTrue();
        }
    }

    // ── A paired-split day only needs the 8-hour split total (s.19 ↔ s.14) ───
    //
    // A genuine team rotation: three 4h sleeper periods chain into pairs (day-one
    // 16:00 ↔ 00:00, 00:00 ↔ 09:00), and every driving leg-pair around a period
    // stays within 13h (s.19(1)(d)): 7+4, 4+5, 5+4. Day two's qualifying off-duty
    // is 9:55 — two 4h sleeper periods plus a 1:55 evening block (the 25-minute
    // breaks don't count) — under the 10-hour s.14 minimum but over the 8-hour
    // team split total. Note the periods must not touch a longer rest: a 4h
    // sleeper inside an 8h+ run is just part of a full rest, not a split period.
    // Day three's off is exactly 10h, so the two-day total (19:55) blocks the
    // s.16 deferral for the single driver: solo flags day two (and the unsplit
    // 20h driving stretch), team reports nothing.
    private static TimelineNavigator.TimelineNavigator BuildEightHourSplitDay() => Build(
        ("1/01/2024 00:00", DutyStatus.OffDuty),   // 8h full rest
        ("1/01/2024 08:00", DutyStatus.Driving),   // 4h
        ("1/01/2024 12:00", DutyStatus.OnDuty),    // 1h
        ("1/01/2024 13:00", DutyStatus.Driving),   // 3h — 7h driving before the first period
        ("1/01/2024 16:00", DutyStatus.Sleeper),   // 4h — pairs with the 00:00 period
        ("1/01/2024 20:00", DutyStatus.Driving),   // 4h (legs 7+4 = 11 ≤ 13)
        ("1/02/2024 00:00", DutyStatus.Sleeper),   // 4h — period 1 of the day
        ("1/02/2024 04:00", DutyStatus.Driving),   // 5h (legs 4+5 = 9 ≤ 13)
        ("1/02/2024 09:00", DutyStatus.Sleeper),   // 4h — period 2 of the day
        ("1/02/2024 13:00", DutyStatus.Driving),   // 4h (legs 5+4 = 9 ≤ 13)
        ("1/02/2024 17:00", DutyStatus.OnDuty),    // 2h
        ("1/02/2024 19:00", DutyStatus.OffDuty),   // 25m — under the block size
        ("1/02/2024 19:25", DutyStatus.OnDuty),    // 2h
        ("1/02/2024 21:25", DutyStatus.OffDuty),   // 25m — under the block size
        ("1/02/2024 21:50", DutyStatus.OnDuty),    // 15m — day-two worked 13:15
        ("1/02/2024 22:05", DutyStatus.OffDuty),   // 1:55 today; run 22:05 → 08:00 = 9:55
        ("1/03/2024 08:00", DutyStatus.Driving),   // 13h
        ("1/03/2024 21:00", DutyStatus.OnDuty),    // 1h — worked 14h
        ("1/03/2024 22:00", DutyStatus.OffDuty),   // 2h today -> day-three off exactly 10h
        ("1/04/2024 08:00", DutyStatus.Unknown));

    [Test]
    public async Task EightHourSplitDay_TeamRuleset_MeetsTheDailyMinimum()
    {
        var sut = new HosAuditor(new CanadaTeamCycle1RuleDefinition());
        var result = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("1/01/2024 01:00"),
            DateTime.Parse("1/04/2024 01:00"),
            BuildEightHourSplitDay(), AuditRules.AllRules));

        await Assert.That(result.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task EightHourSplitDay_SoloRuleset_ThrowsDailyOffDutyViolation()
    {
        var sut = new HosAuditor(new CanadaCycle1RuleDefinition());
        var result = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("1/01/2024 01:00"),
            DateTime.Parse("1/04/2024 01:00"),
            BuildEightHourSplitDay(), AuditRules.AllRules));

        var dailyViolations = result.Violations.Where(v => v.Limit == TimeSpan.FromHours(10)).ToList();
        using (Assert.Multiple())
        {
            await Assert.That(dailyViolations.Count).IsEqualTo(1);
            await Assert.That(dailyViolations[0].StartTimestamp).IsEqualTo(DateTime.Parse("1/02/2024"));
            await Assert.That(dailyViolations[0].TimeInViolation).IsEqualTo(TimeSpan.FromMinutes(5));
        }
    }
}
