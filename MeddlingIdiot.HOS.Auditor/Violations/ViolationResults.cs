namespace MeddlingIdiot.HOS.Violations
{
    public class ViolationResults
    {
        public List<Violation> Violations { get; private set; }
        public ClearViolationRange ClearViolationRange { get; private set; }
        public string DebugInfo { get; private set; }

        public ViolationResults(List<Violation> violations, ClearViolationRange clearViolationRange, string debugInfo)
        {
            Violations = violations;
            ClearViolationRange = clearViolationRange;
            DebugInfo = debugInfo;
        }


    }
}
