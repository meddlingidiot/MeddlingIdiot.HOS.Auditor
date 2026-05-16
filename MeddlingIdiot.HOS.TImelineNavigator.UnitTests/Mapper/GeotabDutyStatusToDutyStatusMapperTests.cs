using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.TimelineNavigator.Mapper;

namespace MeddlingIdiot.HOS.TimelineNavigator.UnitTests.Mapper;

public class GeotabDutyStatusToDutyStatusMapperTests
{
    [Test]
    [Arguments("OFF", DutyStatus.OffDuty)]
    [Arguments("SB",  DutyStatus.Sleeper)]
    [Arguments("D",   DutyStatus.Driving)]
    [Arguments("ON",  DutyStatus.OnDuty)]
    [Arguments("WT",  DutyStatus.OffDutyWaitingAtWellSite)]
    public async Task MapKnownGeotabStatus_ReturnsExpectedDutyStatus(string input, DutyStatus expected)
    {
        await Assert.That(GeotabDutyStatusToDutyStatusMapper.Map(input)).IsEqualTo(expected);
    }

    [Test]
    [Arguments("")]
    [Arguments("off")]
    [Arguments("sb")]
    [Arguments("d")]
    [Arguments("on")]
    [Arguments("wt")]
    [Arguments("UNKNOWN")]
    [Arguments("X")]
    public async Task MapUnknownGeotabStatus_ReturnsUnknown(string input)
    {
        await Assert.That(GeotabDutyStatusToDutyStatusMapper.Map(input)).IsEqualTo(DutyStatus.Unknown);
    }
}
