namespace Automation.HOS.Rules
{
    internal enum ThrowViolations
    {
        AtRestAccumulated,
        AtDutyStatusChange, 
        AtEndOfDay,
        AtEndOfAuditWindow
    }

    internal static class ThrowViolationsAt
    {
        public static List<ThrowViolations> RestAccumulated = new List<ThrowViolations>
        {
            ThrowViolations.AtRestAccumulated,
            ThrowViolations.AtEndOfAuditWindow
        };

        public static List<ThrowViolations> DutyStatusChange = new List<ThrowViolations>
        {
            ThrowViolations.AtDutyStatusChange,
            ThrowViolations.AtEndOfAuditWindow
        };
        public static List<ThrowViolations> EndOfDay = new List<ThrowViolations>
        {
            ThrowViolations.AtEndOfDay,
            ThrowViolations.AtRestAccumulated,
            ThrowViolations.AtEndOfAuditWindow
        };
    }
}
