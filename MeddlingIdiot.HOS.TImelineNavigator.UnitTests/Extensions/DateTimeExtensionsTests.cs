using MeddlingIdiot.HOS.TimelineNavigator.Extensions;

namespace MeddlingIdiot.HOS.TimelineNavigator.UnitTests.Extensions;

public class DateTimeExtensionsTests
{
    [Test]
    public async Task AbsoluteDifference_ReturnsPositiveSpan_WhenStartIsAfterEnd()
    {
        var start = DateTime.Parse("01/27/2023 10:00:00");
        var end   = DateTime.Parse("01/27/2023 08:00:00");

        var result = start.AbsoluteDifference(end);

        await Assert.That(result).IsEqualTo(TimeSpan.FromHours(2));
    }

    [Test]
    public async Task AbsoluteDifference_ReturnsPositiveSpan_WhenEndIsAfterStart()
    {
        var start = DateTime.Parse("01/27/2023 08:00:00");
        var end   = DateTime.Parse("01/27/2023 10:00:00");

        var result = start.AbsoluteDifference(end);

        await Assert.That(result).IsEqualTo(TimeSpan.FromHours(2));
    }

    [Test]
    public async Task AbsoluteDifference_ReturnsZero_WhenStartAndEndAreEqual()
    {
        var timestamp = DateTime.Parse("01/27/2023 08:00:00");

        var result = timestamp.AbsoluteDifference(timestamp);

        await Assert.That(result).IsEqualTo(TimeSpan.Zero);
    }
}
