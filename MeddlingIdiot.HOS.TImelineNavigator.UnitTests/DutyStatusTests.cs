using MeddlingIdiot.HOS.TimelineNavigator;

namespace MeddlingIdiot.HOS.TimelineNavigator.UnitTests;

public class DutyStatusTests
{
    [Test]
    public async Task HaveCorrectEnumValues()
    {
        int unknown = (int)DutyStatus.Unknown;
        int offDuty = (int)DutyStatus.OffDuty;
        int sleeper = (int)DutyStatus.Sleeper;
        int driving = (int)DutyStatus.Driving;
        int onDuty = (int)DutyStatus.OnDuty;
        int offDutyWaitingAtWellSite = (int)DutyStatus.OffDutyWaitingAtWellSite;
        int personalConveyance = (int)DutyStatus.PersonalConveyance;
        int yardMove = (int)DutyStatus.YardMove;

        using (Assert.Multiple())
        {
            await Assert.That(unknown).IsEqualTo(0);
            await Assert.That(offDuty).IsEqualTo(1);
            await Assert.That(sleeper).IsEqualTo(2);
            await Assert.That(driving).IsEqualTo(3);
            await Assert.That(onDuty).IsEqualTo(4);
            await Assert.That(offDutyWaitingAtWellSite).IsEqualTo(5);
            await Assert.That(personalConveyance).IsEqualTo(6);
            await Assert.That(yardMove).IsEqualTo(7);
        }
    }

    [Test]
    public async Task HaveNoDutyStatusesAsEmptyList()
    {
        await Assert.That(DutyStatuses.NoDutyStatuses.Count).IsEqualTo(0);
    }

    [Test]
    public async Task HaveDrivingDutyStatusContainingOnlyDriving()
    {
        using (Assert.Multiple())
        {
            await Assert.That(DutyStatuses.DrivingDutyStatus.Count).IsEqualTo(1);
            await Assert.That(DutyStatuses.DrivingDutyStatus).Contains(DutyStatus.Driving);
        }
    }

    [Test]
    public async Task HaveWorkingDutyStatusesContainingDrivingOnDutyAndYardMove()
    {
        using (Assert.Multiple())
        {
            await Assert.That(DutyStatuses.WorkingDutyStatuses.Count).IsEqualTo(3);
            await Assert.That(DutyStatuses.WorkingDutyStatuses).Contains(DutyStatus.Driving);
            await Assert.That(DutyStatuses.WorkingDutyStatuses).Contains(DutyStatus.OnDuty);
            await Assert.That(DutyStatuses.WorkingDutyStatuses).Contains(DutyStatus.YardMove);
        }
    }

    [Test]
    public async Task HaveRestDutyStatusesContainingOffDutySleeperWellSiteAndPersonalConveyance()
    {
        using (Assert.Multiple())
        {
            await Assert.That(DutyStatuses.RestDutyStatuses.Count).IsEqualTo(4);
            await Assert.That(DutyStatuses.RestDutyStatuses).Contains(DutyStatus.OffDuty);
            await Assert.That(DutyStatuses.RestDutyStatuses).Contains(DutyStatus.Sleeper);
            await Assert.That(DutyStatuses.RestDutyStatuses).Contains(DutyStatus.OffDutyWaitingAtWellSite);
            await Assert.That(DutyStatuses.RestDutyStatuses).Contains(DutyStatus.PersonalConveyance);
        }
    }

    [Test]
    public async Task HaveAllRestDutyStatusesContainingUnknownOffDutySleeperWellSiteAndPersonalConveyance()
    {
        using (Assert.Multiple())
        {
            await Assert.That(DutyStatuses.AllRestDutyStatuses.Count).IsEqualTo(5);
            await Assert.That(DutyStatuses.AllRestDutyStatuses).Contains(DutyStatus.Unknown);
            await Assert.That(DutyStatuses.AllRestDutyStatuses).Contains(DutyStatus.OffDuty);
            await Assert.That(DutyStatuses.AllRestDutyStatuses).Contains(DutyStatus.Sleeper);
            await Assert.That(DutyStatuses.AllRestDutyStatuses).Contains(DutyStatus.OffDutyWaitingAtWellSite);
            await Assert.That(DutyStatuses.AllRestDutyStatuses).Contains(DutyStatus.PersonalConveyance);
        }
    }

    [Test]
    public async Task HaveAllNormalDutyStatusesContainingTheFourBaseStatusesPlusPersonalConveyanceAndYardMove()
    {
        using (Assert.Multiple())
        {
            await Assert.That(DutyStatuses.AllNormalDutyStatuses.Count).IsEqualTo(6);
            await Assert.That(DutyStatuses.AllNormalDutyStatuses).Contains(DutyStatus.OffDuty);
            await Assert.That(DutyStatuses.AllNormalDutyStatuses).Contains(DutyStatus.Sleeper);
            await Assert.That(DutyStatuses.AllNormalDutyStatuses).Contains(DutyStatus.Driving);
            await Assert.That(DutyStatuses.AllNormalDutyStatuses).Contains(DutyStatus.OnDuty);
            await Assert.That(DutyStatuses.AllNormalDutyStatuses).Contains(DutyStatus.PersonalConveyance);
            await Assert.That(DutyStatuses.AllNormalDutyStatuses).Contains(DutyStatus.YardMove);
        }
    }

    [Test]
    public async Task HaveAllButDrivingDutyStatusesContainingEverythingExceptDrivingAndUnknown()
    {
        using (Assert.Multiple())
        {
            await Assert.That(DutyStatuses.AllButDrivingDutyStatuses.Count).IsEqualTo(6);
            await Assert.That(DutyStatuses.AllButDrivingDutyStatuses).Contains(DutyStatus.OffDuty);
            await Assert.That(DutyStatuses.AllButDrivingDutyStatuses).Contains(DutyStatus.Sleeper);
            await Assert.That(DutyStatuses.AllButDrivingDutyStatuses).Contains(DutyStatus.OnDuty);
            await Assert.That(DutyStatuses.AllButDrivingDutyStatuses).Contains(DutyStatus.OffDutyWaitingAtWellSite);
            await Assert.That(DutyStatuses.AllButDrivingDutyStatuses).Contains(DutyStatus.PersonalConveyance);
            await Assert.That(DutyStatuses.AllButDrivingDutyStatuses).Contains(DutyStatus.YardMove);
        }
    }

    [Test]
    public async Task NotContainDrivingInAllButDrivingDutyStatuses()
    {
        await Assert.That(DutyStatuses.AllButDrivingDutyStatuses).DoesNotContain(DutyStatus.Driving);
    }

    [Test]
    public async Task NotContainUnknownInWorkingDutyStatuses()
    {
        await Assert.That(DutyStatuses.WorkingDutyStatuses).DoesNotContain(DutyStatus.Unknown);
    }

    [Test]
    public async Task NotContainDrivingInRestDutyStatuses()
    {
        await Assert.That(DutyStatuses.RestDutyStatuses).DoesNotContain(DutyStatus.Driving);
    }

    [Test]
    public async Task NotContainUnknownInAllNormalDutyStatuses()
    {
        await Assert.That(DutyStatuses.AllNormalDutyStatuses).DoesNotContain(DutyStatus.Unknown);
    }

    [Test]
    public async Task NotContainPersonalConveyanceInWorkingDutyStatuses()
    {
        await Assert.That(DutyStatuses.WorkingDutyStatuses).DoesNotContain(DutyStatus.PersonalConveyance);
    }

    [Test]
    public async Task NotContainYardMoveInRestDutyStatuses()
    {
        using (Assert.Multiple())
        {
            await Assert.That(DutyStatuses.RestDutyStatuses).DoesNotContain(DutyStatus.YardMove);
            await Assert.That(DutyStatuses.AllRestDutyStatuses).DoesNotContain(DutyStatus.YardMove);
        }
    }

    [Test]
    public async Task NotContainPersonalConveyanceOrYardMoveInDrivingDutyStatus()
    {
        using (Assert.Multiple())
        {
            await Assert.That(DutyStatuses.DrivingDutyStatus).DoesNotContain(DutyStatus.PersonalConveyance);
            await Assert.That(DutyStatuses.DrivingDutyStatus).DoesNotContain(DutyStatus.YardMove);
        }
    }
}
