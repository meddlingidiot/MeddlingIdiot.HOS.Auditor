using Automation.HOS.TimelineNavigator;
using Automation.HOS.TimelineNavigator.Moments;
using Automation.HOS.TimelineNavigator.Utilities;
using NUnit.Framework;

namespace Automation.HOS.UnitTests;

[TestFixture]
public class GpsStreamToDutyStatusTimelineTests
{
    private GpsStreamToDutyStatusTimeline _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new GpsStreamToDutyStatusTimeline(new NullLogger());
    }

    [Test]
    public void ThrowsArgumentNullException_WhenNavigatorIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.ConvertGpsTimelineToDutyStatusTimeline(null!));
    }

    [Test]
    public void AddsDriving_WhenDistanceBetweenGpsPointsExceedsThreshold()
    {
        // Euclidean distance sqrt(0.1^2 + 0.1^2) ≈ 0.141, well above the 0.02 threshold
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new GpsMoment(DateTime.Parse("1/1/2024 08:00:00"), 0.0, 0.0));
        navigator.Add(new GpsMoment(DateTime.Parse("1/1/2024 09:00:00"), 0.1, 0.1));

        var result = _sut.ConvertGpsTimelineToDutyStatusTimeline(navigator);

        result.JumpTo(DateTime.Parse("1/1/2024 08:00:00"));
        Assert.That(result.DutyStatus, Is.EqualTo(DutyStatus.Driving));
    }

    [Test]
    public void AddsSleeper_WhenDistanceBetweenGpsPointsIsBelowThreshold()
    {
        // Euclidean distance sqrt(0.001^2 + 0.001^2) ≈ 0.00141, well below the 0.02 threshold
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new GpsMoment(DateTime.Parse("1/1/2024 08:00:00"), 0.0, 0.0));
        navigator.Add(new GpsMoment(DateTime.Parse("1/1/2024 09:00:00"), 0.001, 0.001));

        var result = _sut.ConvertGpsTimelineToDutyStatusTimeline(navigator);

        result.JumpTo(DateTime.Parse("1/1/2024 08:00:00"));
        Assert.That(result.DutyStatus, Is.EqualTo(DutyStatus.Sleeper));
    }

    [Test]
    public void AddsSleeper_WhenDistanceIsExactlyAtThreshold()
    {
        // Euclidean distance = exactly 0.02 (not greater than), so Sleeper
        var delta = 0.02 / Math.Sqrt(2); // both lat and lon offset so distance == 0.02
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new GpsMoment(DateTime.Parse("1/1/2024 08:00:00"), 0.0, 0.0));
        navigator.Add(new GpsMoment(DateTime.Parse("1/1/2024 09:00:00"), delta, delta));

        var result = _sut.ConvertGpsTimelineToDutyStatusTimeline(navigator);

        result.JumpTo(DateTime.Parse("1/1/2024 08:00:00"));
        Assert.That(result.DutyStatus, Is.EqualTo(DutyStatus.Sleeper));
    }

    [Test]
    public void UsesTimestampFromCurrentGpsMoment()
    {
        var gpsTimestamp = DateTime.Parse("3/15/2024 14:30:00");
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new GpsMoment(gpsTimestamp, 0.0, 0.0));
        navigator.Add(new GpsMoment(gpsTimestamp.AddHours(1), 0.1, 0.1));

        var result = _sut.ConvertGpsTimelineToDutyStatusTimeline(navigator);

        result.JumpTo(gpsTimestamp);
        Assert.That(result.CurrentDutyStatusChangeMoment.Timestamp, Is.EqualTo(gpsTimestamp));
    }

    [Test]
    public void ProducesCorrectSequence_ForAlternatingDrivingAndSleeper()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        // 8am→9am: large move → Driving added at 8am
        navigator.Add(new GpsMoment(DateTime.Parse("1/1/2024 08:00:00"), 0.0, 0.0));
        navigator.Add(new GpsMoment(DateTime.Parse("1/1/2024 09:00:00"), 0.1, 0.1));
        // 9am→10am: tiny move → Sleeper added at 9am
        navigator.Add(new GpsMoment(DateTime.Parse("1/1/2024 10:00:00"), 0.101, 0.101));

        var result = _sut.ConvertGpsTimelineToDutyStatusTimeline(navigator);

        result.JumpTo(DateTime.Parse("1/1/2024 08:00:00"));
        Assert.That(result.DutyStatus, Is.EqualTo(DutyStatus.Driving));

        result.JumpTo(DateTime.Parse("1/1/2024 09:00:00"));
        Assert.That(result.DutyStatus, Is.EqualTo(DutyStatus.Sleeper));
    }

    [Test]
    public void ReturnsSameNavigatorInstance()
    {
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new GpsMoment(DateTime.Parse("1/1/2024 08:00:00"), 0.0, 0.0));
        navigator.Add(new GpsMoment(DateTime.Parse("1/1/2024 09:00:00"), 0.1, 0.1));

        var result = _sut.ConvertGpsTimelineToDutyStatusTimeline(navigator);

        Assert.That(result, Is.SameAs(navigator));
    }

    [Test]
    public void PassesDriverIdNumberAsComment_AndTruckNumberAsDriverIdNumber()
    {
        // Note: the production code passes gps.DriverIdNumber as the 'comment' argument
        // and gps.TruckNumber as the 'driverIdNumber' argument of DutyStatusChangeMoment.
        var navigator = new TimelineNavigator.TimelineNavigator(new());
        navigator.Add(new GpsMoment(DateTime.Parse("1/1/2024 08:00:00"), 0.0, 0.0, "D123", "T456"));
        navigator.Add(new GpsMoment(DateTime.Parse("1/1/2024 09:00:00"), 0.1, 0.1, "D123", "T456"));

        var result = _sut.ConvertGpsTimelineToDutyStatusTimeline(navigator);

        result.JumpTo(DateTime.Parse("1/1/2024 08:00:00"));
        Assert.That(result.CurrentDutyStatusChangeMoment.Comment, Is.EqualTo("D123"));
        Assert.That(result.CurrentDutyStatusChangeMoment.DriverIdNumber, Is.EqualTo("T456"));
    }
}
