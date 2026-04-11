using Automation.HOS.Queries;
using Automation.HOS.Ruleset;
using Automation.HOS.TimelineNavigator;
using Automation.HOS.TimelineNavigator.Moments;
using Automation.HOS.TimelineNavigator.Utilities;
using NUnit.Framework;

namespace Automation.HOS.UnitTests
{
    [TestFixture]
    public class HosAuditorShould
    {
        
        [SetUp]
        public void SetUp()
        {
            var categories = new List<string> {LoggerCategories.All};
            //ProMiles.HOS.Utilities.Logger.Instance.Initialize(categories);
        }

        [Test]
        public void AuditViolationsForRange()
        {
            var navigator = new TimelineNavigator.TimelineNavigator(new());
            navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.Driving));         //8
            navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 08:00:00"), DutyStatus.Sleeper));//8
            navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 16:00:00"), DutyStatus.OnDuty)); //2
            navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 18:00:00"), DutyStatus.OffDuty));//3
            navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 21:00:00"), DutyStatus.Driving));//12
            navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 09:00:00"), DutyStatus.OffDuty));

           
            //GPS To DutyStatus notes
            //
            // Set every GPS point to Sleeper or Driving. Then
            // Run a clean out procedure to remove the DutyStatuses that are not needed.

            var sut = new HosAuditor(new Us60HrRuleDefinition()
            );
            var violationResult = sut.AuditRange(new AuditRangeQuery(
                DateTime.Parse("8/24/2023 12:23 PM"), 
                DateTime.Parse("03/08/2024"), 
                navigator, AuditRules.AllRules));

            Assert.That(violationResult.ClearViolationRange.Start, Is.EqualTo(DateTime.Parse("08/24/2023")));
            Assert.That(violationResult.ClearViolationRange.Finish, Is.EqualTo(DateTime.Parse("09/07/2023")));

            Assert.That(violationResult.Violations.Count, Is.EqualTo(2));
            Assert.That(violationResult.Violations[0].Limit, Is.EqualTo(TimeSpan.FromHours(8)));
            Assert.That(violationResult.Violations[0].StartTimestamp, Is.EqualTo(DateTime.Parse("8/25/2023 05:00:00")));
            Assert.That(violationResult.Violations[0].TimeInViolation, Is.EqualTo(TimeSpan.FromHours(4)));
            Assert.That(violationResult.Violations[1].Limit, Is.EqualTo(TimeSpan.FromHours(11)));
            Assert.That(violationResult.Violations[1].StartTimestamp, Is.EqualTo(DateTime.Parse("8/25/2023 08:00:00")));
            Assert.That(violationResult.Violations[1].TimeInViolation, Is.EqualTo(TimeSpan.FromHours(1)));
        }

        [Test]
        public async Task AuditViolationsForPoint()
        {
            var navigator = new TimelineNavigator.TimelineNavigator(new());
            navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023"), DutyStatus.Driving));         //8
            navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 08:00:00"), DutyStatus.Sleeper));//8
            navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 16:00:00"), DutyStatus.OnDuty)); //2
            navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 18:00:00"), DutyStatus.OffDuty));//3
            navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/24/2023 21:00:00"), DutyStatus.Driving));//12
            navigator.Add(new DutyStatusChangeMoment(DateTime.Parse("8/25/2023 09:00:00"), DutyStatus.OffDuty));


            //GPS To DutyStatus notes
            //
            // Set every GPS point to Sleeper or Driving. Then
            // Run a clean out procedure to remove the DutyStatuses that are not needed.

            var sut = new HosAuditor(new Us60HrRuleDefinition()
            );
            var violationResult = await sut.AuditPointAsync(new AuditPointQuery(
                DateTime.Parse("8/24/2023 12:23 PM"),
                navigator, AuditRules.AllRules));

            Assert.That(violationResult.ClearViolationRange.Start, Is.EqualTo(DateTime.Parse("08/24/2023")));
            Assert.That(violationResult.ClearViolationRange.Finish, Is.EqualTo(DateTime.Parse("09/07/2023")));

            Assert.That(violationResult.Violations.Count, Is.EqualTo(2));
            Assert.That(violationResult.Violations[0].Limit, Is.EqualTo(TimeSpan.FromHours(8)));
            Assert.That(violationResult.Violations[0].StartTimestamp, Is.EqualTo(DateTime.Parse("8/25/2023 05:00:00")));
            Assert.That(violationResult.Violations[0].TimeInViolation, Is.EqualTo(TimeSpan.FromHours(4)));
            Assert.That(violationResult.Violations[1].Limit, Is.EqualTo(TimeSpan.FromHours(11)));
            Assert.That(violationResult.Violations[1].StartTimestamp, Is.EqualTo(DateTime.Parse("8/25/2023 08:00:00")));
            Assert.That(violationResult.Violations[1].TimeInViolation, Is.EqualTo(TimeSpan.FromHours(1)));

        }

        [Test]
        public void AuditFor60HourViolations()
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

            var sut = new HosAuditor(new Us60HrRuleDefinition()
            );
            var violationResult = sut.AuditRange(new AuditRangeQuery(
                DateTime.Parse("3/01/2024"),
                DateTime.Parse("03/08/2024"),
                navigator, AuditRules.WindowRules, true));

            Assert.That(violationResult.ClearViolationRange.Start, Is.EqualTo(DateTime.Parse("03/01/2024")));
            Assert.That(violationResult.ClearViolationRange.Finish, Is.EqualTo(DateTime.Parse("03/21/2024")));

            Assert.That(violationResult.Violations.Count, Is.EqualTo(2));
            Assert.That(violationResult.Violations[0].Limit, Is.EqualTo(TimeSpan.FromHours(60)));
            Assert.That(violationResult.Violations[0].StartTimestamp, Is.EqualTo(DateTime.Parse("3/07/2024 10:00:00")));
            Assert.That(violationResult.Violations[0].TimeInViolation, Is.EqualTo(TimeSpan.FromHours(08)));
            Assert.That(violationResult.Violations[1].Limit, Is.EqualTo(TimeSpan.FromHours(60)));
            Assert.That(violationResult.Violations[1].StartTimestamp, Is.EqualTo(DateTime.Parse("3/08/2024 10:00:00")));
            Assert.That(violationResult.Violations[1].TimeInViolation, Is.EqualTo(TimeSpan.FromHours(10)));
        }
    }
}
