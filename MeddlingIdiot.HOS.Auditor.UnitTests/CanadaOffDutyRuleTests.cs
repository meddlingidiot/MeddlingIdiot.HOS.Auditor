using MeddlingIdiot.HOS;
using MeddlingIdiot.HOS.Queries;
using MeddlingIdiot.HOS.Ruleset;
using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;

namespace MeddlingIdiot.HOS.Auditor.UnitTests;

// The Canadian off-duty requirements beyond the shift rules (SOR/2005-313):
//   s.14 — at least 10h off duty per day, counting only blocks ≥ 30 minutes.
//   s.16 — up to 2h of a day's off-duty may be deferred to the next day when the
//          short day keeps its 8 consecutive hours, the two-day off-duty total
//          reaches 20h, the next day's consecutive rest absorbs the deferred time,
//          and the two-day driving total stays within 26h.
//   s.25 — no driving without 24 consecutive hours off in the preceding 14 days.
//   s.27(b) — Cycle 2 only: no driving after 70h on duty in the cycle without
//          first taking 24 consecutive hours off.
public class CanadaOffDutyRuleTests
{
    private static TimelineNavigator.TimelineNavigator Build(params (string Timestamp, DutyStatus DutyStatus)[] points)
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        foreach (var (timestamp, dutyStatus) in points)
            navigator.Add(new DutyStatusChangeMoment(DateTime.Parse(timestamp), dutyStatus));
        return navigator;
    }

    // ── s.14: daily off-duty minimum ─────────────────────────────────────────

    // Day 1 totals 9.5h off (its two 25-minute breaks don't count toward the
    // minimum); day 2 totals 7.5h. Deferral can't excuse day 1 because the two-day
    // off-duty total is only 17h (< 20h), and day 2 lacks even its 8 consecutive.
    private static TimelineNavigator.TimelineNavigator BuildTwoShortDays() => Build(
        ("8/23/2023 16:00", DutyStatus.OffDuty),
        ("8/24/2023 08:00", DutyStatus.Driving),   // 5h
        ("8/24/2023 13:00", DutyStatus.OffDuty),   // 25m — under the 30-minute block size
        ("8/24/2023 13:25", DutyStatus.Driving),   // 5h
        ("8/24/2023 18:25", DutyStatus.OffDuty),   // 25m — under the 30-minute block size
        ("8/24/2023 18:50", DutyStatus.OnDuty),    // 3h40m — worked 13:40 (< 14)
        ("8/24/2023 22:30", DutyStatus.OffDuty),   // 1.5h today; the run reaches 06:30 (8h)
        ("8/25/2023 06:30", DutyStatus.Driving),   // 5h
        ("8/25/2023 11:30", DutyStatus.OnDuty),    // 3.5h
        ("8/25/2023 15:00", DutyStatus.Driving),   // 4h
        ("8/25/2023 19:00", DutyStatus.OnDuty),    // 4h (never drives over any limit)
        ("8/25/2023 23:00", DutyStatus.OffDuty),   // 1h today
        ("8/26/2023 04:00", DutyStatus.OffDuty));

    [Test]
    public async Task DailyOffDuty_TwoShortDays_ThrowsAViolationPerDay()
    {
        var sut = new HosAuditor(new CanadaCycle1RuleDefinition());
        var result = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 01:00"),
            DateTime.Parse("8/26/2023 01:00"),
            BuildTwoShortDays(), AuditRules.AllRules));

        var dailyViolations = result.Violations
            .Where(v => v.Limit == TimeSpan.FromHours(10))
            .OrderBy(v => v.StartTimestamp)
            .ToList();

        using (Assert.Multiple())
        {
            await Assert.That(result.Violations.Count).IsEqualTo(2);
            await Assert.That(dailyViolations.Count).IsEqualTo(2);
            await Assert.That(dailyViolations[0].StartTimestamp).IsEqualTo(DateTime.Parse("8/24/2023"));
            await Assert.That(dailyViolations[0].TimeInViolation).IsEqualTo(TimeSpan.FromMinutes(30)); // 9.5h of 10h
            await Assert.That(dailyViolations[1].StartTimestamp).IsEqualTo(DateTime.Parse("8/25/2023"));
            await Assert.That(dailyViolations[1].TimeInViolation).IsEqualTo(TimeSpan.FromHours(2.5));  // 7.5h of 10h
        }
    }

    [Test]
    public async Task DailyOffDuty_SameTimelineUnderUsRules_NoDailyViolations()
    {
        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var result = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 01:00"),
            DateTime.Parse("8/26/2023 01:00"),
            BuildTwoShortDays(), AuditRules.AllRules));

        // The US ruleset has no daily off-duty minimum; whatever else it flags
        // (breaks, driving limits), no 10-hour daily violation may appear.
        await Assert.That(result.Violations.Any(v => v.Limit == TimeSpan.FromHours(10))).IsFalse();
    }

    // ── s.16: deferral ───────────────────────────────────────────────────────

    // Day 1 totals 9:40 off (20-minute shortfall deferred). Every s.16 condition
    // holds: day 1 keeps 8 consecutive hours (22:20–06:30 spans midnight), day 2's
    // consecutive rest is 8:40 (≥ 8h + the 20m deferred), the two-day off-duty
    // total is 21:40 (≥ 20h), and two-day driving is 18h (≤ 26h).
    [Test]
    public async Task DailyOffDuty_LegalDeferralToNextDay_NoViolations()
    {
        var navigator = Build(
            ("8/23/2023 16:00", DutyStatus.OffDuty),
            ("8/24/2023 08:00", DutyStatus.Driving),   // 5h
            ("8/24/2023 13:00", DutyStatus.OnDuty),    // 4h
            ("8/24/2023 17:00", DutyStatus.Driving),   // 4h — driving 9h, worked 13h
            ("8/24/2023 21:00", DutyStatus.OffDuty),   // 20m — under the block size
            ("8/24/2023 21:20", DutyStatus.OnDuty),    // 1h — worked 14h (at, not over, the limit)
            ("8/24/2023 22:20", DutyStatus.OffDuty),   // 1:40 today → 9:40 total; run spans to 07:00 (8:40)
            ("8/25/2023 07:00", DutyStatus.Driving),   // 5h
            ("8/25/2023 12:00", DutyStatus.OnDuty),    // 3h
            ("8/25/2023 15:00", DutyStatus.Driving),   // 4h — two-day driving 18h
            ("8/25/2023 19:00", DutyStatus.OffDuty),   // 5h today → day 2 total 12h
            ("8/26/2023 04:00", DutyStatus.OffDuty));

        var sut = new HosAuditor(new CanadaCycle1RuleDefinition());
        var result = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 01:00"),
            DateTime.Parse("8/26/2023 01:00"),
            navigator, AuditRules.AllRules));

        await Assert.That(result.Violations.Count).IsEqualTo(0);
    }

    // ── s.25: 24 consecutive hours off in the preceding 14 days ─────────────

    // Identical work days with 14h off each night — never 24 consecutive. The
    // unknown history before the data counts as rest, so the requirement first
    // bites 14 days after the data starts (minus the 24h): driving from
    // 1/14 08:00 onward violates. On-duty is 10h/day, so the rolling 7-day cycle
    // sits exactly at 70h — never over — isolating the s.25 rule.
    private static TimelineNavigator.TimelineNavigator BuildRepeatingDays(int totalDays, int? fullyOffDay)
    {
        var points = new List<(string, DutyStatus)>();
        for (int dayNumber = 1; dayNumber <= totalDays; dayNumber++)
        {
            if (fullyOffDay == dayNumber)
                continue; // remain in the previous evening's off-duty through this day
            var date = new DateTime(2024, 1, dayNumber);
            points.Add(($"{date:M/d/yyyy} 00:00", DutyStatus.OffDuty));
            points.Add(($"{date:M/d/yyyy} 08:00", DutyStatus.Driving));
            points.Add(($"{date:M/d/yyyy} 13:00", DutyStatus.OnDuty));
            points.Add(($"{date:M/d/yyyy} 18:00", DutyStatus.OffDuty));
        }
        points.Add(($"1/{totalDays + 1}/2024 00:00", DutyStatus.Unknown));
        return Build(points.ToArray());
    }

    [Test]
    public async Task ExtendedRestLookback_FourteenDaysWithoutTwentyFourOff_ThrowsPerDrivingStretch()
    {
        var sut = new HosAuditor(new CanadaCycle1RuleDefinition());
        var result = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("1/02/2024 01:00"),
            DateTime.Parse("1/17/2024 12:00"),
            BuildRepeatingDays(17, null), AuditRules.AllRules));

        var lookbackViolations = result.Violations
            .Where(v => v.Limit == TimeSpan.FromHours(24))
            .OrderBy(v => v.StartTimestamp)
            .ToList();

        using (Assert.Multiple())
        {
            await Assert.That(result.Violations.Count).IsEqualTo(4);
            await Assert.That(lookbackViolations.Count).IsEqualTo(4); // days 14-17, one per 5h driving stretch
            await Assert.That(lookbackViolations[0].StartTimestamp).IsEqualTo(DateTime.Parse("1/14/2024 08:00"));
            await Assert.That(lookbackViolations[0].TimeInViolation).IsEqualTo(TimeSpan.FromHours(5));
            await Assert.That(lookbackViolations[3].StartTimestamp).IsEqualTo(DateTime.Parse("1/17/2024 08:00"));
        }
    }

    [Test]
    public async Task ExtendedRestLookback_TwentyFourOffMidway_NoViolations()
    {
        var sut = new HosAuditor(new CanadaCycle1RuleDefinition());
        var result = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("1/02/2024 01:00"),
            DateTime.Parse("1/17/2024 12:00"),
            BuildRepeatingDays(17, fullyOffDay: 8), AuditRules.AllRules));

        await Assert.That(result.Violations.Count).IsEqualTo(0);
    }

    // ── s.27(b): Cycle 2's 70h-without-24h-off gate ──────────────────────────

    // 10h of driving per day accumulates 70h on duty by the end of day 7; the
    // driving on days 8 and 9 happens without any 24h off-duty period in the
    // cycle, violating s.27(b) even though 90h is well under the 120h/14-day
    // cycle limit.
    private static TimelineNavigator.TimelineNavigator BuildCycle2Days(int totalDays, int? fullyOffDay)
    {
        var points = new List<(string, DutyStatus)>();
        for (int dayNumber = 1; dayNumber <= totalDays; dayNumber++)
        {
            if (fullyOffDay == dayNumber)
                continue;
            var date = new DateTime(2024, 1, dayNumber);
            points.Add(($"{date:M/d/yyyy} 00:00", DutyStatus.OffDuty));
            points.Add(($"{date:M/d/yyyy} 08:00", DutyStatus.Driving));
            points.Add(($"{date:M/d/yyyy} 18:00", DutyStatus.OffDuty));
        }
        points.Add(($"1/{totalDays + 1}/2024 00:00", DutyStatus.Unknown));
        return Build(points.ToArray());
    }

    [Test]
    public async Task Cycle2_SeventyHoursOnDutyWithoutTwentyFourOff_ThrowsWhenDrivingContinues()
    {
        var sut = new HosAuditor(new CanadaCycle2RuleDefinition());
        var result = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("1/02/2024 01:00"),
            DateTime.Parse("1/09/2024 12:00"),
            BuildCycle2Days(9, null), AuditRules.AllRules));

        var midCycleViolations = result.Violations
            .Where(v => v.Limit == TimeSpan.FromHours(70))
            .OrderBy(v => v.StartTimestamp)
            .ToList();

        using (Assert.Multiple())
        {
            await Assert.That(result.Violations.Count).IsEqualTo(2);
            await Assert.That(midCycleViolations.Count).IsEqualTo(2); // days 8 and 9
            await Assert.That(midCycleViolations[0].StartTimestamp).IsEqualTo(DateTime.Parse("1/08/2024 08:00"));
            await Assert.That(midCycleViolations[0].TimeInViolation).IsEqualTo(TimeSpan.FromHours(10));
            await Assert.That(midCycleViolations[1].StartTimestamp).IsEqualTo(DateTime.Parse("1/09/2024 08:00"));
        }
    }

    [Test]
    public async Task Cycle2_TwentyFourOffBeforeSeventyHours_NoViolations()
    {
        var sut = new HosAuditor(new CanadaCycle2RuleDefinition());
        var result = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("1/02/2024 01:00"),
            DateTime.Parse("1/10/2024 12:00"),
            BuildCycle2Days(10, fullyOffDay: 4), AuditRules.AllRules));

        // Day 4 off makes a 38h rest — an extended rest (≥ 24h) but not a 72h
        // cycle reset — so the 90h that follow are legal under s.27(b).
        await Assert.That(result.Violations.Count).IsEqualTo(0);
    }
}
