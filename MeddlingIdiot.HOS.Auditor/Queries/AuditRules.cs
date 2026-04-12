namespace MeddlingIdiot.HOS.Queries
{
    public enum AuditRule
    {
        UnbrokenDriving,
        Driving,
        Shift,
        Window
    }

    public static class AuditRules
    {
        public static List<AuditRule> AllRules = new List<AuditRule>
        {
            AuditRule.UnbrokenDriving,
            AuditRule.Driving,
            AuditRule.Shift,
            AuditRule.Window
        };

        public static List<AuditRule> UnbrokenDrivingRules = new List<AuditRule>
        {
            AuditRule.UnbrokenDriving
        };

        public static List<AuditRule> DrivingRules = new List<AuditRule>
        {
            AuditRule.Driving
        };

        public static List<AuditRule> ShiftRules = new List<AuditRule>
        {
            AuditRule.Shift
        };

        public static List<AuditRule> WindowRules = new List<AuditRule>
        {
            AuditRule.Window
        };
    }
}
