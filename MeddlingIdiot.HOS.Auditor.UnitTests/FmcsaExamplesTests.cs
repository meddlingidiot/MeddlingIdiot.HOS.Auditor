using System.Reflection;
using MeddlingIdiot.HOS;
using MeddlingIdiot.HOS.Queries;
using MeddlingIdiot.HOS.Ruleset;
using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;
using MeddlingIdiot.HOS.TimelineNavigator.Segments;
using Assembly = System.Reflection.Assembly;

namespace MeddlingIdiot.HOS.Auditor.UnitTests;

//Built using the FMCSA examples from the following document:
//https://www.fmcsa.dot.gov/sites/fmcsa.dot.gov/files/2022-04/FMCSA-HOS-395-HOS-EXAMPLES%282022-04-28%29_0.pdf

public class FmcsaExamplesTests
{
    private string? _currentWorkingDirectory;

    [Before(Test)]
    public Task SetUp(TestContext context)
    {
        _currentWorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        return Task.CompletedTask;
    }

    [Test]
    public async Task Example01_14HourDrivingWindow_NoViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OnDuty));         //1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 01:00:00"), DutyStatus.Driving));//5
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 06:00:00"), DutyStatus.OffDuty));//1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 07:00:00"), DutyStatus.Driving));//3
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.OnDuty)); //2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 12:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 14:00:00"), DutyStatus.OffDuty));//1

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 AM"),
            DateTime.Parse("8/24/2023 12:25 AM"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_01.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/07/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Example02_10HourConsecutiveHourOffDutyBreak_NoViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 11:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 16:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 17:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 22:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 23:00:00"), DutyStatus.Sleeper));
        //Day 2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 07:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 08:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 09:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 13:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 14:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 18:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 20:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 22:00:00"), DutyStatus.OffDuty));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 AM"),
            DateTime.Parse("8/25/2023 12:25 AM"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_02.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/07/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Example03_DrivingLimit_NoViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 11:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 16:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 17:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 23:00:00"), DutyStatus.OnDuty));
        //Day 2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023"), DutyStatus.Sleeper));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 10:00:00"), DutyStatus.Unknown));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 AM"),
            DateTime.Parse("8/25/2023 12:25 AM"),
            navigator, AuditRules.AllRules));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_03.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/07/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Example04_DrivingLimit_WithViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 01:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 05:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 06:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 11:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 15:00:00"), DutyStatus.OffDuty));
        //Day 2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 01:00:00"), DutyStatus.Unknown));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 AM"),
            DateTime.Parse("8/25/2023 12:25 AM"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_04.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/07/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(2);
        await Assert.That(violationResult.Violations[0].Limit).IsEqualTo(TimeSpan.FromHours(11));
        await Assert.That(violationResult.Violations[0].StartTimestamp).IsEqualTo(DateTime.Parse("08/24/2023 14:00:00"));
        await Assert.That(violationResult.Violations[0].EndTimestamp).IsEqualTo(DateTime.Parse("08/24/2023 15:00:00"));
        await Assert.That(violationResult.Violations[1].Limit).IsEqualTo(TimeSpan.FromHours(14));
        await Assert.That(violationResult.Violations[1].StartTimestamp).IsEqualTo(DateTime.Parse("08/24/2023 14:00:00"));
        await Assert.That(violationResult.Violations[1].EndTimestamp).IsEqualTo(DateTime.Parse("08/24/2023 15:00:00"));
    }

    [Test]
    public async Task Example05_RestBreaks_NoViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 01:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 09:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 09:15:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 09:30:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 11:30:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 13:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 14:00:00"), DutyStatus.OffDuty));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023 12:23 AM"),
            DateTime.Parse("8/25/2023 12:25 AM"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_05.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/06/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Example06_34HourRestart_NoViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        //Day 2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 10:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 16:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 17:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 21:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 23:00:00"), DutyStatus.Driving));
        //Day 3
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023 10:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023 15:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023 16:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023 21:00:00"), DutyStatus.Driving));
        //EOT
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/27/2023"), DutyStatus.Unknown));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/25/2023 17:23"),
            DateTime.Parse("8/25/2023 17:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_06.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/09/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Example07_34HourRestartMixedRest_NoViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 01:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 03:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 05:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 11:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 12:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 13:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 21:00:00"), DutyStatus.Sleeper));
        //Day 2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 08:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 23:00:00"), DutyStatus.Driving));
        //Day 3
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023 04:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023 05:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023 09:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023 10:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023 12:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023 14:00:00"), DutyStatus.OffDuty));
        //EOT
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/27/2023"), DutyStatus.Unknown));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/26/2023 10:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_07.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023 00:00:00"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/09/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Example08_34HourRestartMultiday_NoViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 11:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 17:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 18:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 19:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 20:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 22:00:00"), DutyStatus.OnDuty));
        //Day 2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023"), DutyStatus.Sleeper));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 10:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 11:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 15:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 17:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 20:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 21:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 23:00:00"), DutyStatus.OnDuty));
        //Day 3
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023 10:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023 12:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023 15:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023 17:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023 19:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023 21:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023 23:00:00"), DutyStatus.OnDuty));
        //Day 4
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/27/2023"), DutyStatus.Sleeper));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/27/2023 10:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/27/2023 11:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/27/2023 14:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/27/2023 15:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/27/2023 18:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/27/2023 19:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/27/2023 22:00:00"), DutyStatus.OnDuty));
        //Day 5
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/28/2023"), DutyStatus.Sleeper));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/28/2023 05:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/28/2023 10:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/28/2023 12:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/28/2023 15:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/28/2023 16:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/28/2023 17:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/28/2023 18:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/28/2023 19:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/28/2023 20:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/28/2023 22:00:00"), DutyStatus.OnDuty));
        //Day 6
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/29/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/29/2023 10:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/29/2023 11:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/29/2023 15:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/29/2023 20:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/29/2023 21:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/29/2023 23:00:00"), DutyStatus.OnDuty));
        //Day 7
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/30/2023"), DutyStatus.OffDuty));
        //Day 8
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/31/2023 10:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/31/2023 12:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/31/2023 16:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/31/2023 17:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/31/2023 21:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/31/2023 22:00:00"), DutyStatus.OnDuty));
        //EO
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("9/01/2023"), DutyStatus.Unknown));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/31/2023 10:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_08.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023 00:00:00"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/14/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Example09_16HourDrivingWindow_NoViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 11:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 13:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 14:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 17:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 18:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 19:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 20:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 23:00:00"), DutyStatus.OnDuty));
        //Day 2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 02:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 12:00:00"), DutyStatus.Unknown));

        navigator.Upsert(new ShiftExtensionSegment(DateTime.Parse("8/25/2023"),
            DateTime.Parse("8/25/2023 02:00:00")));
        navigator.Upsert(new ShiftExtensionSegment(DateTime.Parse("9/01/2023 02:00:00"),
            DateTime.Parse("9/01/2023 04:00:00")));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/24/2023 10:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_09.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023 00:00:00"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/14/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Example10_16HourDrivingWindow_WithViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 12:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 16:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 17:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 20:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 21:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 22:00:00"), DutyStatus.OnDuty));
        //Day 2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 03:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 13:00:00"), DutyStatus.Unknown));

        navigator.Upsert(new ShiftExtensionSegment(DateTime.Parse("8/25/2023"),
                           DateTime.Parse("8/25/2023 02:00:00")));
        navigator.Upsert(new ShiftExtensionSegment(DateTime.Parse("9/01/2023 02:01:00"),
            DateTime.Parse("9/01/2023 04:00:00")));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/24/2023 10:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_10.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023 00:00:00"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/14/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(3);
        await Assert.That(violationResult.Violations[0].Limit).IsEqualTo(TimeSpan.FromHours(14));
        await Assert.That(violationResult.Violations[0].StartTimestamp).IsEqualTo(DateTime.Parse("8/25/2023 02:00:00"));
        await Assert.That(violationResult.Violations[0].EndTimestamp).IsEqualTo(DateTime.Parse("8/25/2023 03:00:00"));
        await Assert.That(violationResult.Violations[0].TimeInViolation).IsEqualTo(TimeSpan.FromHours(1));
        await Assert.That(violationResult.Violations[1].Limit).IsEqualTo(TimeSpan.FromDays(7));
        await Assert.That(violationResult.Violations[1].StartTimestamp).IsEqualTo(DateTime.Parse("8/25/2023 02:00:00"));
        await Assert.That(violationResult.Violations[1].EndTimestamp).IsEqualTo(DateTime.Parse("9/01/2023 02:01:00"));
        await Assert.That(violationResult.Violations[1].TimeInViolation).IsEqualTo(TimeSpan.Parse("07.00:01:00"));
        await Assert.That(violationResult.Violations[2].Limit).IsEqualTo(TimeSpan.FromDays(7));
        await Assert.That(violationResult.Violations[2].StartTimestamp).IsEqualTo(DateTime.Parse("9/01/2023 02:01:00"));
        await Assert.That(violationResult.Violations[2].EndTimestamp).IsEqualTo(DateTime.Parse("9/08/2023 02:02:00"));
        await Assert.That(violationResult.Violations[2].TimeInViolation).IsEqualTo(TimeSpan.Parse("07.00:01:00"));
    }

    [Test]
    public async Task Example11_TwoDriverPropertyCarringCmv_NoViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 02:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 06:00:00"), DutyStatus.Sleeper));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 14:00:00"), DutyStatus.OffDuty, "In passenger seat of moving vehicle"));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 16:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 17:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 20:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 21:00:00"), DutyStatus.Driving));
        //EOT
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023"), DutyStatus.Unknown));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = sut.AuditRange(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/24/2023 10:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_11.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023 00:00:00"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/07/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Example12_SleeperBerthUse_NoViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:15:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 15:15:00"), DutyStatus.OffDuty, "In passenger seat of moving vehicle"));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 16:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 22:00:00"), DutyStatus.Sleeper));
        //Day 2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 05:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 08:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 08:30:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 13:30:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 14:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 20:00:00"), DutyStatus.OffDuty));
        //EOT
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023"), DutyStatus.Unknown));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/24/2023 10:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_12.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023 00:00:00"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/07/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Example13_TwoDriverProperCarryingCmv_WithViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 02:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 03:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 09:00:00"), DutyStatus.OnDuty, "In passenger seat of moving vehicle"));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 11:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 15:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 16:00:00"), DutyStatus.Sleeper));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 23:00:00"), DutyStatus.OffDuty));
        //Day 2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 01:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 02:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 07:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 08:00:00"), DutyStatus.OnDuty, "In passenger seat of moving vehicle"));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 09:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 12:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 13:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 16:00:00"), DutyStatus.OffDuty));
        //EOT
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023"), DutyStatus.Unknown));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/24/2023 10:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_13.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023 00:00:00"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/07/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(1);
        await Assert.That(violationResult.Violations[0].Limit).IsEqualTo(TimeSpan.FromHours(14));
        await Assert.That(violationResult.Violations[0].StartTimestamp).IsEqualTo(DateTime.Parse("08/25/2023 15:00:00"));
        await Assert.That(violationResult.Violations[0].EndTimestamp).IsEqualTo(DateTime.Parse("08/25/2023 16:00:00"));
        await Assert.That(violationResult.Violations[0].TimeInViolation).IsEqualTo(TimeSpan.FromHours(1));
    }

    [Test]
    public async Task Example14_SleeperBerthUseWitRestBreak_WithViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:15:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 14:15:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 14:30:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 19:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 22:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 23:00:00"), DutyStatus.Driving));
        //Day 2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 00:30:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 02:00:00"), DutyStatus.Sleeper));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 09:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 09:30:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 17:30:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 18:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 18:30:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 21:30:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 22:00:00"), DutyStatus.Driving));
        //EOT
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023"), DutyStatus.Unknown));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/24/2023 10:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_14.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023 00:00:00"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/07/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(1);
        await Assert.That(violationResult.Violations[0].Limit).IsEqualTo(TimeSpan.FromHours(8));
        await Assert.That(violationResult.Violations[0].StartTimestamp).IsEqualTo(DateTime.Parse("08/24/2023 18:30:00"));
        await Assert.That(violationResult.Violations[0].TimeInViolation).IsEqualTo(TimeSpan.FromMinutes(30));
        await Assert.That(violationResult.Violations[0].EndTimestamp).IsEqualTo(DateTime.Parse("08/24/2023 19:00:00"));
    }

    [Test]
    public async Task Example15_SleeperBerthUseNotValidSplit_WithViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 15:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 17:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 20:00:00"), DutyStatus.Sleeper));
        //Day 2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 03:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 07:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 17:00:00"), DutyStatus.Unknown));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/24/2023 10:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_15.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023 00:00:00"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/07/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(1);
        await Assert.That(violationResult.Violations[0].Limit).IsEqualTo(TimeSpan.FromHours(11));
        await Assert.That(violationResult.Violations[0].StartTimestamp).IsEqualTo(DateTime.Parse("08/25/2023 06:00:00"));
        await Assert.That(violationResult.Violations[0].TimeInViolation).IsEqualTo(TimeSpan.FromHours(1));
    }

    [Test]
    public async Task Example16_SleeperBerthProperUse_NoViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 00:30:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 05:30:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 09:30:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 15:30:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 16:00:00"), DutyStatus.Sleeper));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 23:00:00"), DutyStatus.OnDuty));
        //Day 2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 05:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 06:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 09:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 13:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 15:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 16:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 17:00:00"), DutyStatus.Sleeper));
        //EOT
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023"), DutyStatus.Unknown));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/24/2023 10:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_16.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023 00:00:00"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/08/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Example17_SleeperBerthPairWithFullRestSleeperSegment_NoViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 11:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 16:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 18:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 19:00:00"), DutyStatus.Driving));
        //Day 2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 01:00:00"), DutyStatus.Sleeper));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 11:00:00"), DutyStatus.Unknown));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/24/2023 10:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_17.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023 00:00:00"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/07/2023"));
        //TODO: Bug Clear Range should be the lowest of the two numbers :timeline<DutyStatus>.Max() or end of the audit range.
        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Example18_SleeperBerthMultiplePairings_NoViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 13:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 16:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 17:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 19:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 21:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 22:00:00"), DutyStatus.Driving));
        //Day 2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 03:00:00"), DutyStatus.Sleeper));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 11:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 16:00:00"), DutyStatus.OffDuty));
        //EOT
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023"), DutyStatus.Unknown));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/24/2023 10:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_18.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023 00:00:00"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/08/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Example19_WaitingTimeAtWellSiteExample1_NoViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 11:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 16:00:00"), DutyStatus.OffDutyWaitingAtWellSite));
        //Day 2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 05:00:00"), DutyStatus.Unknown));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/24/2023 10:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_19.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023 00:00:00"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/07/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Example20_WaitingTimeAtWellSiteExample2_NoViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 11:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 16:00:00"), DutyStatus.OffDutyWaitingAtWellSite));
        //Day 2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 05:00:00"), DutyStatus.Unknown));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/24/2023 10:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_20.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023 00:00:00"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/07/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Example21_SplitBreakWithWellTime_WithViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 13:00:00"), DutyStatus.Sleeper));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 17:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 20:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 23:00:00"), DutyStatus.OffDutyWaitingAtWellSite));
        //Day 2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 00:30:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 04:00:00"), DutyStatus.OffDutyWaitingAtWellSite));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 10:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 15:30:00"), DutyStatus.Sleeper));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 19:30:00"), DutyStatus.Unknown));

        var sut = new HosAuditor(new UsOilfield60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/24/2023 10:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_21.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023 00:00:00"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/07/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(2);
        await Assert.That(violationResult.Violations[0].Limit).IsEqualTo(TimeSpan.FromHours(11));
        await Assert.That(violationResult.Violations[0].StartTimestamp).IsEqualTo(DateTime.Parse("08/25/2023 14:30:00"));
        await Assert.That(violationResult.Violations[0].EndTimestamp).IsEqualTo(DateTime.Parse("08/25/2023 15:30:00"));
        await Assert.That(violationResult.Violations[0].TimeInViolation).IsEqualTo(TimeSpan.FromHours(1));
        await Assert.That(violationResult.Violations[1].Limit).IsEqualTo(TimeSpan.FromHours(14));
        await Assert.That(violationResult.Violations[1].StartTimestamp).IsEqualTo(DateTime.Parse("08/25/2023 14:30:00"));
        await Assert.That(violationResult.Violations[1].EndTimestamp).IsEqualTo(DateTime.Parse("08/25/2023 15:30:00"));
        await Assert.That(violationResult.Violations[1].TimeInViolation).IsEqualTo(TimeSpan.FromHours(1));
    }

    [Test]
    public async Task Example22_OilfieldWithWellWaitTime_NoViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 14:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 16:00:00"), DutyStatus.OffDutyWaitingAtWellSite));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 17:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 20:00:00"), DutyStatus.OffDutyWaitingAtWellSite));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 22:00:00"), DutyStatus.Driving));
        //Day 2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 03:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 13:00:00"), DutyStatus.Unknown));

        var sut = new HosAuditor(new UsBus60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/24/2023 10:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_22.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023 00:00:00"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/07/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Example23_AgriculturalOperationException_WithViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 01:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 06:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 07:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 08:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 18:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 21:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 21:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 23:00:00"), DutyStatus.OnDuty));
        //Day 2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023"), DutyStatus.Unknown));

        navigator.Upsert(new AgriculturalExceptionSegment(DateTime.Parse("8/24/2023 09:00:00"), DateTime.Parse("8/24/2023 17:00:00")));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/24/2023 10:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_23.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023 00:00:00"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/07/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(1);
        await Assert.That(violationResult.Violations[0].Limit).IsEqualTo(TimeSpan.FromHours(14));
        await Assert.That(violationResult.Violations[0].StartTimestamp).IsEqualTo(DateTime.Parse("08/24/2023 22:00:00"));
        await Assert.That(violationResult.Violations[0].EndTimestamp).IsEqualTo(DateTime.Parse("08/24/2023 23:00:00"));
        await Assert.That(violationResult.Violations[0].TimeInViolation).IsEqualTo(TimeSpan.FromHours(1));
    }

    [Test]
    public async Task Example24_AgriculturalOperationException_NoViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 04:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 09:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 09:15:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 15:00:00"), DutyStatus.Sleeper));
        //Day 2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 09:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 09:15:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 15:00:00"), DutyStatus.OnDuty));
        //EOT
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023"), DutyStatus.Unknown));

        navigator.Upsert(new AgriculturalExceptionSegment(DateTime.Parse("8/24/2023 04:00:00"), DateTime.Parse("8/24/2023 09:00:00")));
        navigator.Upsert(new AgriculturalExceptionSegment(DateTime.Parse("8/25/2023 04:00:00"), DateTime.Parse("8/25/2023 09:00:00")));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/24/2023 10:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_24.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023 00:00:00"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/08/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Example25_PassengerCarryingVehicles_NoViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 02:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 12:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 18:00:00"), DutyStatus.Sleeper));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 20:00:00"), DutyStatus.Driving));
        //EOT
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023"), DutyStatus.Unknown));

        var sut = new HosAuditor(new UsBus60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/24/2023 10:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_25.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023 00:00:00"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/07/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Example26_PassengerCarryingVehicles_NoViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 08:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 14:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 15:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 17:00:00"), DutyStatus.Sleeper));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 18:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 20:00:00"), DutyStatus.OffDuty));
        //Day 2
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 01:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 02:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 05:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 06:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 08:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 16:00:00"), DutyStatus.Driving));
        //EOT
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023"), DutyStatus.Unknown));

        var sut = new HosAuditor(new UsBus60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/24/2023 10:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_26.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023 00:00:00"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/08/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Example27_PassengerCarryingVehicles_WithViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 02:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 04:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 05:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 07:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 08:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 13:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 15:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 19:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 20:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 21:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 22:00:00"), DutyStatus.Driving));
        //EOT
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023"), DutyStatus.Unknown));

        var sut = new HosAuditor(new UsBus60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/24/2023 10:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_27.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023 00:00:00"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/07/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(1);
        await Assert.That(violationResult.Violations[0].Limit).IsEqualTo(TimeSpan.FromHours(15));
        await Assert.That(violationResult.Violations[0].StartTimestamp).IsEqualTo(DateTime.Parse("08/24/2023 22:00:00"));
        await Assert.That(violationResult.Violations[0].EndTimestamp).IsEqualTo(DateTime.Parse("08/25/2023"));
        await Assert.That(violationResult.Violations[0].TimeInViolation).IsEqualTo(TimeSpan.FromHours(2));
    }

    [Test]
    public async Task Example28_PassengerCarryingVehicles_WithViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 04:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 05:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 12:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 13:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 18:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 19:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 20:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 21:00:00"), DutyStatus.OffDuty));
        //EOT
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023"), DutyStatus.Unknown));

        var sut = new HosAuditor(new UsBus60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/24/2023 10:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_28.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023 00:00:00"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/07/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(1);
        await Assert.That(violationResult.Violations[0].Limit).IsEqualTo(TimeSpan.FromHours(10));
        await Assert.That(violationResult.Violations[0].StartTimestamp).IsEqualTo(DateTime.Parse("08/24/2023 20:00:00"));
        await Assert.That(violationResult.Violations[0].EndTimestamp).IsEqualTo(DateTime.Parse("08/24/2023 21:00:00"));
        await Assert.That(violationResult.Violations[0].TimeInViolation).IsEqualTo(TimeSpan.FromHours(1));
    }

    [Test]
    public async Task Example29_60_70_HourRule_WithViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1 Sunday
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        //Day 2 Monday
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 10:00:00"), DutyStatus.OffDuty));
        //Day 3 Tuesday
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/26/2023 08:30:00"), DutyStatus.OffDuty));
        //Day 4 Wednesday
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/27/2023"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/27/2023 12:30:00"), DutyStatus.OffDuty));
        //Day 5 Thursday
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/28/2023"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/28/2023 09:00:00"), DutyStatus.OffDuty));
        //Day 6 Friday
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/29/2023"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/29/2023 10:00:00"), DutyStatus.OffDuty));
        //Day 7 Saturday
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/30/2023"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/30/2023 12:00:00"), DutyStatus.OffDuty));
        //Day 8 Sunday
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/31/2023"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/31/2023 05:00:00"), DutyStatus.OffDuty));
        //Day 9 Monday
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("9/01/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("9/01/2023 10:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("9/01/2023 12:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("9/01/2023 13:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("9/01/2023 14:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("9/01/2023 15:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("9/01/2023 16:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("9/01/2023 19:00:00"), DutyStatus.OffDuty));
        //EOT
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("9/02/2023"), DutyStatus.Unknown));

        var sut = new HosAuditor(new Us70HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("9/01/2023"),
            DateTime.Parse("9/01/2023 10:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_29.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023 00:00:00"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/15/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(1);
        await Assert.That(violationResult.Violations[0].Limit).IsEqualTo(TimeSpan.FromHours(70));
        await Assert.That(violationResult.Violations[0].StartTimestamp).IsEqualTo(DateTime.Parse("09/01/2023 15:00:00"));
        await Assert.That(violationResult.Violations[0].EndTimestamp).IsEqualTo(DateTime.Parse("09/01/2023 16:00:00"));
        await Assert.That(violationResult.Violations[0].TimeInViolation).IsEqualTo(TimeSpan.FromHours(1));
    }

    [Test]
    public async Task Example30_AdverseDrivingConditionsException_NoViolations()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        //Day 1
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 05:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 09:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 10:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 13:00:00"), DutyStatus.OnDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 14:00:00"), DutyStatus.OffDuty));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 15:00:00"), DutyStatus.Driving));
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 21:00:00"), DutyStatus.OffDuty));
        //EOT
        navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023"), DutyStatus.Unknown));

        navigator.Upsert(new AdverseConditionsSegment(DateTime.Parse("8/24/2023 13:00:00"), DateTime.Parse("8/24/2023 14:00:00")));

        var sut = new HosAuditor(new Us60HrRuleDefinition());
        var violationResult = await sut.AuditRangeAsync(new AuditRangeQuery(
            DateTime.Parse("8/24/2023"),
            DateTime.Parse("8/24/2023 10:25"),
            navigator, AuditRules.AllRules, true));

        await File.WriteAllTextAsync(_currentWorkingDirectory + "\\FMCSA_Example_30.log", violationResult.DebugInfo);

        await Assert.That(violationResult.ClearViolationRange.Start).IsEqualTo(DateTime.Parse("08/24/2023 00:00:00"));
        await Assert.That(violationResult.ClearViolationRange.Finish).IsEqualTo(DateTime.Parse("09/07/2023"));
        await Assert.That(violationResult.Violations.Count).IsEqualTo(0);
    }
}
