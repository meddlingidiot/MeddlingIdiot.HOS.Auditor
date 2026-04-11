using Automation.HOS.RestTimelineBuilders;
using Automation.HOS.Ruleset;
using Automation.HOS.TimelineNavigator;
using Automation.HOS.TimelineNavigator.Moments;
using Automation.HOS.TimelineNavigator.Timelines;
using Automation.HOS.TimelineNavigator.Utilities;
using NUnit.Framework;

namespace Automation.HOS.UnitTests.RestTimelineBuilders
{
    [TestFixture]
    public class RestTimelinePairerUsaPrimaryShould
    {
        private void PopulateTimeline(ITimelineNavigator data)
        {
            data.Add(new DutyStatusChangeMoment(DateTime.Parse("02/14/2023 08:00:00"), DutyStatus.OnDuty));
            //ON-DUTY 10 HOURS
            data.Add(new DutyStatusChangeMoment(DateTime.Parse("02/14/2023 18:00:00"), DutyStatus.OffDuty));
            //OFF DUTY 1 HOURS
            data.Add(new DutyStatusChangeMoment(DateTime.Parse("02/14/2023 19:00:00"), DutyStatus.Sleeper));
            //SLEEPER 8 HOUR (TOTAL REST 9 HOURS)
            data.Add(new DutyStatusChangeMoment(DateTime.Parse("02/15/2023 03:00:00"), DutyStatus.OnDuty));
            //ON DUTY 1 HOURS
            data.Add(new DutyStatusChangeMoment(DateTime.Parse("02/15/2023 04:00:00"), DutyStatus.OffDuty));
            //OFF DUTY 4 HOURS (Pairs)
            data.Add(new DutyStatusChangeMoment(DateTime.Parse("02/15/2023 08:00:00"), DutyStatus.OnDuty));
            //ON DUTY Rest of day
            data.Add(new DutyStatusChangeMoment(DateTime.Parse("02/16/2023 04:00:00"), DutyStatus.Sleeper));
            //SLEEPER 4 HOURS
            data.Add(new DutyStatusChangeMoment(DateTime.Parse("02/16/2023 08:00:00"), DutyStatus.OnDuty));
            //ON DUTY Rest of day
            data.Add(new DutyStatusChangeMoment(DateTime.Parse("02/17/2023 00:00:00"), DutyStatus.Unknown));
            //UNKNOWN TIL EOT
        }

        [SetUp]
        public void SetUp()
        {
            //Logger.Instance.Initialize(categories);
        }

        [Test]
        public async Task BuildNormalHappyPath()
        {
            var logger = new InMemoryLogger();
            var categories = new List<string> { LoggerCategories.Pairing };
            logger.Initialize(categories);

            var nav = new TimelineNavigator.TimelineNavigator(new StartOfDayTimelineOptions(new DateTime(0)));
            PopulateTimeline(nav);

            var rule = new Us60HrRuleDefinition();
            var restTimelineBuilder = new RestTimelineBuilderUsaPrimary(logger, rule, nav); 
            
            var sut = new RestTimelinePairerUsaPrimary(logger, rule, nav);

            restTimelineBuilder.BuildTimeline();
            logger.Debug(LoggerCategories.Pairing, "Before Pairing -------------------------------");
            nav.DumpSplitRestTimeline(logger);
            sut.PairSleeperSplits();
            logger.Debug(LoggerCategories.Pairing, "After Pairing -------------------------------");
            nav.DumpSplitRestTimeline(logger);

            nav.JumpTo(DateTime.Parse("02/14/2023 19:00:00"));
            Assert.That(nav.CurrentRestMoment.IsPrimary, Is.True);
            Assert.That(nav.CurrentRestMoment.IsPaired, Is.True);

            nav.JumpTo(DateTime.Parse("02/15/2023 04:00:00"));
            Assert.That(nav.CurrentRestMoment.Timestamp, Is.EqualTo(DateTime.Parse("02/15/2023 04:00:00")));
            Assert.That(nav.CurrentRestMoment.IsPrimary, Is.False);
            Assert.That(nav.CurrentRestMoment.IsPaired, Is.True);
            Assert.That(nav.DutyStatus, Is.EqualTo(DutyStatus.OffDuty));

            nav.JumpTo(DateTime.Parse("02/15/2023 04:00:00"));
            Assert.That(nav.CurrentRestMoment.Timestamp, Is.EqualTo(DateTime.Parse("02/15/2023 04:00:00")));
            Assert.That(nav.CurrentRestMoment.IsPrimary, Is.False);
            Assert.That(nav.CurrentRestMoment.IsPaired, Is.True);

            //await logger.SaveToFileAsync("c:\\code\\RestTimelinePairerUsaPrimaryShould.BuildNormalHappyPath.log");
        }

        //TODO Write more Tests to fill the wholes.
    }
}
