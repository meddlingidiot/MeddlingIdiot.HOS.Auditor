using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;

namespace MeddlingIdiot.HOS.TimelineNavigator.UnitTests.Moments;

public class DutyChangeMomentTests
{
    [Test]
    public async Task HaveDefaultPropertyValues()
    {
        var sut = new DutyStatusChangeMoment();
        using (Assert.Multiple())
        {
            await Assert.That(sut.Timestamp).IsEqualTo(DateTime.MinValue);
            await Assert.That(sut.DriverIdNumber).IsNull();
            await Assert.That(sut.CurrentDutyStatus).IsEqualTo(DutyStatus.Unknown);
        }
    }
}
