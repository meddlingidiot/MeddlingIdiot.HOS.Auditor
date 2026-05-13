using MeddlingIdiot.HOS.Queries;

namespace MeddlingIdiot.HOS.Violations
{
    public class ViolationResults
    {
        public List<Violation> Violations { get; private set; }
        public ClearViolationRange ClearViolationRange { get; private set; }
        public string DebugInfo { get; private set; }

        public List<DaySummary> DaySummaries { get; } = new();
        public RestTargets? RestTargets { get; internal set; }

        public ViolationResults(List<Violation> violations, ClearViolationRange clearViolationRange, string debugInfo, List<DaySummary>? daySummaries = null, RestTargets? restTargets = null)
        {
            Violations = violations;
            ClearViolationRange = clearViolationRange;
            DebugInfo = debugInfo;
            
            DaySummaries = daySummaries ?? DaySummaries;
            RestTargets = restTargets;
        }


    }
}
