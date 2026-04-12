using MeddlingIdiot.Dispatcher;
using MeddlingIdiot.HOS.TimelineNavigator;
using MeddlingIdiot.HOS.Violations;

namespace MeddlingIdiot.HOS.Queries
{
    public class AuditRangeQuery  : IRequest<ViolationResults>
    {
        public DateTime StartTimestamp { get; private set; }
        public DateTime FinishTimestamp { get; private set; }
        public ITimelineNavigator Navigator { get; private set; }
        public List<AuditRule> Rules { get; private set; }
        public bool IncludeDebugInfo { get; private set; }
        
        public AuditRangeQuery(DateTime startTimestamp, 
            DateTime finishTimestamp, ITimelineNavigator navigator, List<AuditRule>? rulesToAudit = null, bool includeDebugInfo = false)
        {
            if (startTimestamp > finishTimestamp)
            {
                throw new ArgumentException("Start timestamp must be less than finish timestamp.");
            }

            if (navigator == null)
            {
                throw new ArgumentNullException(nameof(navigator));
            }

            StartTimestamp = startTimestamp;
            FinishTimestamp = finishTimestamp;
            Navigator = navigator;
            Rules = rulesToAudit ?? AuditRules.AllRules;
            IncludeDebugInfo = includeDebugInfo;
        }
    }
}
