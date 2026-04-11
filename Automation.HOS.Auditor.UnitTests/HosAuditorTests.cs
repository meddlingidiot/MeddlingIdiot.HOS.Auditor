using Automation.HOS.Queries;
using Automation.HOS.Ruleset;
using Automation.HOS.TimelineNavigator;
using Automation.HOS.TimelineNavigator.Moments;

namespace Automation.HOS.Auditor.UnitTests;

public class HosAuditorTests
{
    [Test]
    public async Task AuditRange_ReturnsViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.Driving));         //8
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 08:00:00"), DutyStatus.Sleeper));//8
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 16:00:00"), DutyStatus.OnDuty)); //2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 18:00:00"), DutyStatus.OffDuty));//3
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 21:00:00"), DutyStatus.Driving));//12
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 09:00:00"), DutyStatus.OffDuty));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = sut.AuditRange(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 PM"),
            DateTime.Parse("03/08/2024"),
            navigator, AuditRules.AllRules));

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/07/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(2);
        await Assert.That(violationResult.Violations[0].Limit).IsEqualTo(TimeSpan.FromHours(8));
        await Assert.That(violationResult.Violations[0].StartTimestamp).IsEqualTo(DateTime.Parse("8/25/2023 05:00:00"));
        await Assert.That(violationResult.Violations[0].TimeInViolation).IsEqualTo(TimeSpan.FromHours(4));
        await Assert.That(violationResult.Violations[1].Limit).IsEqualTo(TimeSpan.FromHours(11));
        await Assert.That(violationResult.Violations[1].StartTimestamp).IsEqualTo(DateTime.Parse("8/25/2023 08:00:00"));
        await Assert.That(violationResult.Violations[1].TimeInViolation).IsEqualTo(TimeSpan.FromHours(1));
    }

    [Test]
    public async Task AuditRangeAsync_ReturnsViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.Driving));         //8
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 08:00:00"), DutyStatus.Sleeper));//8
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 16:00:00"), DutyStatus.OnDuty)); //2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 18:00:00"), DutyStatus.OffDuty));//3
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 21:00:00"), DutyStatus.Driving));//12
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 09:00:00"), DutyStatus.OffDuty));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 PM"),
            DateTime.Parse("03/08/2024"),
            navigator, AuditRules.AllRules));

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/07/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(2);
        await Assert.That(violationResult.Violations[0].Limit).IsEqualTo(TimeSpan.FromHours(8));
        await Assert.That(violationResult.Violations[0].StartTimestamp).IsEqualTo(DateTime.Parse("8/25/2023 05:00:00"));
        await Assert.That(violationResult.Violations[0].TimeInViolation).IsEqualTo(TimeSpan.FromHours(4));
        await Assert.That(violationResult.Violations[1].Limit).IsEqualTo(TimeSpan.FromHours(11));
        await Assert.That(violationResult.Violations[1].StartTimestamp).IsEqualTo(DateTime.Parse("8/25/2023 08:00:00"));
        await Assert.That(violationResult.Violations[1].TimeInViolation).IsEqualTo(TimeSpan.FromHours(1));
    }

    [Test]
    public async Task AuditPointAsync_ReturnsViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.Driving));         //8
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 08:00:00"), DutyStatus.Sleeper));//8
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 16:00:00"), DutyStatus.OnDuty)); //2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 18:00:00"), DutyStatus.OffDuty));//3
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 21:00:00"), DutyStatus.Driving));//12
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 09:00:00"), DutyStatus.OffDuty));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditPointAsync(new AuditPointQuery(
            DateTime.Parse("8/24/2023 12:23 PM"),
            navigator, AuditRules.AllRules));

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/07/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(2);
        await Assert.That(violationResult.Violations[0].Limit).IsEqualTo(TimeSpan.FromHours(8));
        await Assert.That(violationResult.Violations[0].StartTimestamp).IsEqualTo(DateTime.Parse("8/25/2023 05:00:00"));
        await Assert.That(violationResult.Violations[0].TimeInViolation).IsEqualTo(TimeSpan.FromHours(4));
        await Assert.That(violationResult.Violations[1].Limit).IsEqualTo(TimeSpan.FromHours(11));
        await Assert.That(violationResult.Violations[1].StartTimestamp).IsEqualTo(DateTime.Parse("8/25/2023 08:00:00"));
        await Assert.That(violationResult.Violations[1].TimeInViolation).IsEqualTo(TimeSpan.FromHours(1));
    }

    [Test]
    public async Task AuditRange_WindowRule_ReturnsViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());

        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("03/01/2024 08:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("03/01/2024 10:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("03/01/2024 18:00:00"), DutyStatus.OffDuty));

        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("03/02/2024 08:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("03/02/2024 10:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("03/02/2024 18:00:00"), DutyStatus.OffDuty));

        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("03/03/2024 08:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("03/03/2024 10:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("03/03/2024 18:00:00"), DutyStatus.OffDuty));

        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("03/04/2024 08:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("03/04/2024 10:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("03/04/2024 18:00:00"), DutyStatus.OffDuty));

        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("03/05/2024 08:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("03/05/2024 10:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("03/05/2024 18:00:00"), DutyStatus.OffDuty));

        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("03/06/2024 08:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("03/06/2024 10:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("03/06/2024 18:00:00"), DutyStatus.OffDuty));

        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("03/07/2024 08:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("03/07/2024 10:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("03/07/2024 18:00:00"), DutyStatus.OffDuty));

        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("03/08/2024 08:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("03/08/2024 10:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("03/08/2024 20:00:00"), DutyStatus.OffDuty));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = sut.AuditRange(new AuditRangeQuery(
            DateTime.Parse("3/01/2024"),
            DateTime.Parse("03/08/2024"),
            navigator, AuditRules.WindowRules, true));

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("03/01/2024"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("03/21/2024"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(2);
        await Assert.That(violationResult.Violations[0].Limit).IsEqualTo(TimeSpan.FromHours(60));
        await Assert.That(violationResult.Violations[0].StartTimestamp).IsEqualTo(DateTime.Parse("3/07/2024 10:00:00"));
        await Assert.That(violationResult.Violations[0].TimeInViolation).IsEqualTo(TimeSpan.FromHours(8));
        await Assert.That(violationResult.Violations[1].Limit).IsEqualTo(TimeSpan.FromHours(60));
        await Assert.That(violationResult.Violations[1].StartTimestamp).IsEqualTo(DateTime.Parse("3/08/2024 10:00:00"));
        await Assert.That(violationResult.Violations[1].TimeInViolation).IsEqualTo(TimeSpan.FromHours(10));
    }

    [Test]
    public async Task AuditRange_NoViolations_ReturnsEmptyViolationList()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 08:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 16:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023"), DutyStatus.Unknown));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = sut.AuditRange(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/24/2023 16:00:00"),
            navigator, AuditRules.AllRules));

        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task AuditRange_DebugInfoIncluded_WhenRequested()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 08:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023"), DutyStatus.Unknown));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = sut.AuditRange(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/24/2023 08:00:00"),
            navigator, AuditRules.AllRules, true));

        await Assert.That(violationResult.DebugInfo).IsNotNull();
        await Assert.That(violationResult.DebugInfo).IsNotEmpty();
    }

    [Test]
    public async Task AuditRange_DebugInfoEmpty_WhenNotRequested()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 08:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023"), DutyStatus.Unknown));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = sut.AuditRange(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/24/2023 08:00:00"),
            navigator, AuditRules.AllRules));

        await Assert.That(violationResult.DebugInfo).IsEmpty();
    }
}
