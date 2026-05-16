namespace MeddlingIdiot.HOS.TimelineNavigator.Mapper
{
    public static class GeotabDutyStatusToDutyStatusMapper
    {
        public static DutyStatus Map(string geotabDutyStatus)
        {
            switch (geotabDutyStatus)
            {
                case "OFF":
                    return DutyStatus.OffDuty;
                case "SB":
                    return DutyStatus.Sleeper;
                case "D":
                    return DutyStatus.Driving;
                case "ON":
                    return DutyStatus.OnDuty;
                case "WT":
                    return DutyStatus.OffDutyWaitingAtWellSite;
                default:
                    return DutyStatus.Unknown;
            }
        }
    }
}