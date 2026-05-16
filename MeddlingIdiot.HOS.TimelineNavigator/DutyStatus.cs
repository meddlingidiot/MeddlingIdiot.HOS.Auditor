namespace MeddlingIdiot.HOS.TimelineNavigator
{
    public enum DutyStatus
    {
        Unknown = 0,
        OffDuty = 1,
        Sleeper = 2,
        Driving = 3,
        OnDuty = 4,
        OffDutyWaitingAtWellSite = 5

    }

    public static class DutyStatuses
    {
        public static List<DutyStatus> NoDutyStatuses = new List<DutyStatus> { };
        public static List<DutyStatus> DrivingDutyStatus = new List<DutyStatus> { DutyStatus.Driving };
        public static List<DutyStatus> WorkingDutyStatuses = new List<DutyStatus> { DutyStatus.Driving, DutyStatus.OnDuty };
        public static List<DutyStatus> RestDutyStatuses = new List<DutyStatus> { DutyStatus.OffDuty, DutyStatus.Sleeper, DutyStatus.OffDutyWaitingAtWellSite };
        public static List<DutyStatus> AllRestDutyStatuses = new List<DutyStatus> { DutyStatus.Unknown, DutyStatus.OffDuty, DutyStatus.Sleeper, DutyStatus.OffDutyWaitingAtWellSite };
        public static List<DutyStatus> AllNormalDutyStatuses = new List<DutyStatus> { DutyStatus.OffDuty, DutyStatus.Sleeper, DutyStatus.Driving, DutyStatus.OnDuty };
        public static List<DutyStatus> AllButDrivingDutyStatuses = new List<DutyStatus> {  DutyStatus.OffDuty, DutyStatus.Sleeper, DutyStatus.OnDuty, DutyStatus.OffDutyWaitingAtWellSite };

    }

}
