using MeddlingIdiot.HOS.Queries;
using MeddlingIdiot.HOS.TimelineNavigator.Moments;

namespace MeddlingIdiot.HOS.Violations
{
    public class ViolationResults
    {
        public List<Violation> Violations { get; private set; }
        public ClearViolationRange ClearViolationRange { get; private set; }
        public string DebugInfo { get; private set; }

        public List<DaySummary> DaySummaries { get; } = new();
        public List<RestMoment> RestMoments { get; } = new();
        
        public ViolationResults(List<Violation> violations, ClearViolationRange clearViolationRange, string debugInfo, List<DaySummary>? daySummaries = null, List<RestMoment>? restMoments = null)
        {
            Violations = violations;
            ClearViolationRange = clearViolationRange;
            DebugInfo = debugInfo;
            
            DaySummaries = daySummaries ?? DaySummaries;
            RestMoments = restMoments ?? RestMoments;
        }


    }
}
