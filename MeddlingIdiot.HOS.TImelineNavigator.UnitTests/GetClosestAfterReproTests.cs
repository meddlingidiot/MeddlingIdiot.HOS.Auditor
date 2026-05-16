using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;
using MeddlingIdiot.HOS.TimelineNavigator.Timelines;

namespace MeddlingIdiot.HOS.TimelineNavigator.UnitTests;

public class GetClosestAfterReproTests
{
    [Test]
    public async Task GetClosestAfter_ShouldNotReturnMinValue_WhenNoMomentsAfterSearchTimestamp()
    {
        // Arrange
        var options = new StartOfDayTimelineOptions(stopAtStartOfDay: false);
        var navigator = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(options);
        var searchTime = new DateTime(2024, 1, 1, 12, 0, 0);

        // Add a moment in the past
        navigator.Add(new DutyStatusChangeMoment(searchTime.AddHours(-1), DutyStatus.OffDuty));
        navigator.Initialize();

        // We want to jump to the moment we added
        navigator.JumpTo(searchTime.AddHours(-1));

        // At this point Start should be the moment we added (11:00).
        // GetClosestAfter(11:00) should find nothing after it.

        // Assert
        await Assert.That(navigator.Finish.Timestamp).IsEqualTo(DateTime.MaxValue);
    }

    [Test]
    public async Task GetClosestAfter_ShouldOnlyReturnMomentsAfterSearchTimestamp()
    {
        // Arrange
        var options = new StartOfDayTimelineOptions(stopAtStartOfDay: false);
        var navigator = new MeddlingIdiot.HOS.TimelineNavigator.TimelineNavigator(options);
        var baseTime = new DateTime(2024, 1, 1, 12, 0, 0);

        // All timelines have DateTime.MinValue by default.
        // We add a moment at baseTime.
        navigator.Add(new DutyStatusChangeMoment(baseTime, DutyStatus.OnDuty));
        navigator.Initialize();

        // Jump to baseTime
        navigator.JumpTo(baseTime);

        // Start is now baseTime.
        // GetClosestAfter(baseTime) is called to set Finish.
        // It should NOT return DateTime.MinValue.

        await Assert.That(navigator.Finish.Timestamp).IsEqualTo(DateTime.MaxValue);
    }
}
