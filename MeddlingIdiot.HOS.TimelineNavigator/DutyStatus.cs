namespace MeddlingIdiot.HOS.TimelineNavigator
{
    public enum DutyStatus
    {
        Unknown = 0,
        OffDuty = 1,
        Sleeper = 2,
        Driving = 3,
        OnDuty = 4,
        OffDutyWaitingAtWellSite = 5,

        /// <summary>
        /// ELD special driving category "authorized personal use" (49 CFR §395.8
        /// guidance): the CMV is moving but the driver is off duty. Audits exactly
        /// like <see cref="OffDuty"/> — rest for every rule; never driving/working.
        /// The distinct value exists so recording/display layers can draw it as a
        /// driving segment on the off-duty line.
        /// </summary>
        PersonalConveyance = 6,

        /// <summary>
        /// ELD special driving category "yard moves": the CMV is moving inside a
        /// yard, driver on duty but not "driving" for HOS purposes. Audits exactly
        /// like <see cref="OnDuty"/> — working for every rule; never accumulates
        /// driving time.
        /// </summary>
        YardMove = 7

    }

    public static class DutyStatuses
    {
        public static List<DutyStatus> NoDutyStatuses = new List<DutyStatus> { };
        public static List<DutyStatus> DrivingDutyStatus = new List<DutyStatus> { DutyStatus.Driving };
        public static List<DutyStatus> WorkingDutyStatuses = new List<DutyStatus> { DutyStatus.Driving, DutyStatus.OnDuty, DutyStatus.YardMove };
        public static List<DutyStatus> RestDutyStatuses = new List<DutyStatus> { DutyStatus.OffDuty, DutyStatus.Sleeper, DutyStatus.OffDutyWaitingAtWellSite, DutyStatus.PersonalConveyance };
        public static List<DutyStatus> AllRestDutyStatuses = new List<DutyStatus> { DutyStatus.Unknown, DutyStatus.OffDuty, DutyStatus.Sleeper, DutyStatus.OffDutyWaitingAtWellSite, DutyStatus.PersonalConveyance };
        public static List<DutyStatus> AllNormalDutyStatuses = new List<DutyStatus> { DutyStatus.OffDuty, DutyStatus.Sleeper, DutyStatus.Driving, DutyStatus.OnDuty, DutyStatus.PersonalConveyance, DutyStatus.YardMove };
        public static List<DutyStatus> AllButDrivingDutyStatuses = new List<DutyStatus> {  DutyStatus.OffDuty, DutyStatus.Sleeper, DutyStatus.OnDuty, DutyStatus.OffDutyWaitingAtWellSite, DutyStatus.PersonalConveyance, DutyStatus.YardMove };

    }

}
