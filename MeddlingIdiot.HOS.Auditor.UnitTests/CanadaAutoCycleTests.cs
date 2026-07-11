using MeddlingIdiot.HOS;
using MeddlingIdiot.HOS.Queries;
using MeddlingIdiot.HOS.Ruleset;
using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;

namespace MeddlingIdiot.HOS.Auditor.UnitTests;

// Automatic cycle election (SOR/2005-313 s.26–s.29): the audit passes if ANY legal
// sequence of cycle declarations — including the optional s.28 resets and s.29
// switches (Cycle 1 → Cycle 2 after 36h off, Cycle 2 → Cycle 1 after 72h off, hours
// zeroed) — explains the timeline, and throws only when driving occurs with no
// compliant election left.
public class CanadaAutoCycleTests
{
    [Test]
    public async Task AutoCycle_HasBothCyclesConfigured()
    {
        var sut = new CanadaAutoCycleRuleDefinition();

        using (Assert.Multiple())
        {
            await Assert.That(sut.Cycle1WindowLimit).IsEqualTo(TimeSpan.FromHours(70));    // s.26
            await Assert.That(sut.Cycle1DaysInWindow).IsEqualTo(7);
            await Assert.That(sut.Cycle1CycleReset).IsEqualTo(TimeSpan.FromHours(36));     // s.28(a)/s.29
            await Assert.That(sut.Cycle2WindowLimit).IsEqualTo(TimeSpan.FromHours(120));   // s.27(a)
            await Assert.That(sut.Cycle2DaysInWindow).IsEqualTo(14);
            await Assert.That(sut.Cycle2CycleReset).IsEqualTo(TimeSpan.FromHours(72));     // s.28(b)/s.29
            await Assert.That(sut.Cycle2OnDutyLimitWithoutExtendedRest).IsEqualTo(TimeSpan.FromHours(70)); // s.27(b)
            // The plain window rule stays alive for day summaries but must not throw,
            // and the unconditional s.27(b) checker is off (it binds per feasible state).
            await Assert.That(sut.WindowRuleThrowsViolations).IsFalse();
            await Assert.That(sut.MinOnDutyLimitWithoutExtendedRest).IsEqualTo(TimeSpan.Zero);
            // Shift rules match the declared-cycle definitions.
            await Assert.That(sut.MinDrivingLimit).IsEqualTo(TimeSpan.FromHours(13));
            await Assert.That(sut.MinOnDutyLimit).IsEqualTo(TimeSpan.FromHours(14));
            await Assert.That(sut.MinFullRest).IsEqualTo(TimeSpan.FromHours(8));
        }
    }

    // ── The switching scenario ────────────────────────────────────────────────
    //
    // Stretch A (days 1–7): 84h on duty in 7 days — four 14h days, a 34h rest
    //   (≥ 24h, satisfying the s.27(b) gate, but < 36h so no Cycle 1 reset), then
    //   two more 14h days. Legal ONLY under Cycle 2 (84 > 70).
    // 82h off (days 8–10): ends every cycle; permits any switch.
    // Stretch B (days 11–21): two blocks of five 14h days separated by a 36h rest.
    //   Each block is exactly 70h/7d — legal under Cycle 1 with its 36h reset —
    //   but the 14-day window catches 140h > 120h, so B is illegal under Cycle 2.
    //
    // No single declared cycle passes the whole timeline; "Cycle 2, then switch to
    // Cycle 1 during the 82h rest" does. Daily driving is exactly 13h and daily
    // on-duty exactly 14h (at, never over, the limits), daily off-duty is exactly
    // 10h, and every long rest also serves s.25 — isolating the cycle rules.
    private static TimelineNavigator.TimelineNavigator BuildSwitchingScenario()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        void Work(int day, int startHour)
        {
            var date = new DateTime(2024, 1, day);
            navigator.Add(new DutyStatusChangeMoment(date.AddHours(startHour), DutyStatus.Driving));      // 13h
            navigator.Add(new DutyStatusChangeMoment(date.AddHours(startHour + 13), DutyStatus.OnDuty));  // 1h
            navigator.Add(new DutyStatusChangeMoment(date.AddHours(startHour + 14), DutyStatus.OffDuty));
        }

        navigator.Add(new DutyStatusChangeMoment(new DateTime(2024, 1, 1), DutyStatus.OffDuty));
        foreach (var day in new[] { 1, 2, 3, 4 }) Work(day, 8);   // 56h; rest 1/04 22:00 → 1/06 08:00 = 34h
        foreach (var day in new[] { 6, 7 }) Work(day, 8);         // 84h in days 1-7; rest 1/07 22:00 → 1/11 08:00 = 82h
        foreach (var day in new[] { 11, 12, 13, 14, 15 }) Work(day, 8); // 70h; rest 1/15 22:00 → 1/17 10:00 = 36h
        foreach (var day in new[] { 17, 18, 19, 20, 21 }) Work(day, 10); // 70h
        navigator.Add(new DutyStatusChangeMoment(new DateTime(2024, 1, 22, 8, 0, 0), DutyStatus.Unknown));
        return navigator;
    }

    private static AuditRangeQuery Query(TimelineNavigator.TimelineNavigator navigator) => new(
        DateTime.Parse("1/02/2024 01:00"),
        DateTime.Parse("1/21/2024 12:00"),
        navigator, AuditRules.AllRules);

    [Test]
    public async Task SwitchingScenario_AutoCycle_NoViolations()
    {
        var sut = new HosAuditor(new CanadaAutoCycleRuleDefinition());
        var result = await sut.AuditRangeAsync(Query(BuildSwitchingScenario()));

        await Assert.That(result.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task SwitchingScenario_DeclaredCycle1_FlagsStretchA()
    {
        var sut = new HosAuditor(new CanadaCycle1RuleDefinition());
        var result = await sut.AuditRangeAsync(Query(BuildSwitchingScenario()));

        using (Assert.Multiple())
        {
            await Assert.That(result.Violations.Count).IsGreaterThan(0);
            await Assert.That(result.Violations.All(v => v.Limit == TimeSpan.FromHours(70))).IsTrue();
            // Only day 7 (hours 70→84 of the 7-day window) is over; stretch B stays clean.
            await Assert.That(result.Violations.All(v =>
                v.StartTimestamp >= DateTime.Parse("1/07/2024") &&
                v.StartTimestamp < DateTime.Parse("1/08/2024"))).IsTrue();
        }
    }

    [Test]
    public async Task SwitchingScenario_DeclaredCycle2_FlagsStretchB()
    {
        var sut = new HosAuditor(new CanadaCycle2RuleDefinition());
        var result = await sut.AuditRangeAsync(Query(BuildSwitchingScenario()));

        using (Assert.Multiple())
        {
            await Assert.That(result.Violations.Count).IsGreaterThan(0);
            await Assert.That(result.Violations.All(v => v.Limit == TimeSpan.FromHours(120))).IsTrue();
            // The 14-day window (anchored by the 82h reset) crosses 120h eight hours
            // into day 20's driving: 10:00 + 8h = 18:00.
            await Assert.That(result.Violations.Any(v =>
                v.StartTimestamp == DateTime.Parse("1/20/2024 18:00"))).IsTrue();
        }
    }

    // ── No feasible election ─────────────────────────────────────────────────
    //
    // Ten straight 14h days with only 10h off each night. Cycle 1 is over from
    // day 6 (70h accumulate in days 1–5). Cycle 2's 14-day window crosses 120h
    // eight hours into day 9's driving (112h + 8h at 16:00). From that moment no
    // election is compliant, so the violations are exactly the driving from
    // day 9 16:00 (5h to 21:00) and day 10's full 13h stint.
    [Test]
    public async Task NoFeasibleElection_ThrowsFromTheMomentBothCyclesAreOver()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new DutyStatusChangeMoment(new DateTime(2024, 1, 1), DutyStatus.OffDuty));
        for (int day = 1; day <= 10; day++)
        {
            var date = new DateTime(2024, 1, day);
            navigator.Add(new DutyStatusChangeMoment(date.AddHours(8), DutyStatus.Driving));
            navigator.Add(new DutyStatusChangeMoment(date.AddHours(21), DutyStatus.OnDuty));
            navigator.Add(new DutyStatusChangeMoment(date.AddHours(22), DutyStatus.OffDuty));
        }
        navigator.Add(new DutyStatusChangeMoment(new DateTime(2024, 1, 11, 8, 0, 0), DutyStatus.Unknown));

        var sut = new HosAuditor(new CanadaAutoCycleRuleDefinition());
        var result = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("1/02/2024 01:00"),
            DateTime.Parse("1/10/2024 12:00"),
            navigator, AuditRules.AllRules));

        var ordered = result.Violations.OrderBy(v => v.StartTimestamp).ToList();
        using (Assert.Multiple())
        {
            await Assert.That(result.Violations.Count).IsEqualTo(2);
            await Assert.That(ordered[0].StartTimestamp).IsEqualTo(DateTime.Parse("1/09/2024 16:00"));
            await Assert.That(ordered[0].TimeInViolation).IsEqualTo(TimeSpan.FromHours(5));
            await Assert.That(ordered[1].StartTimestamp).IsEqualTo(DateTime.Parse("1/10/2024 08:00"));
            await Assert.That(ordered[1].TimeInViolation).IsEqualTo(TimeSpan.FromHours(13));
            await Assert.That(ordered.All(v => v.Comment.Contains("every cycle election"))).IsTrue();
        }
    }
}
