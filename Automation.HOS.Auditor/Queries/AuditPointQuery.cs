using Automation.Dispatcher;
using Automation.HOS.TimelineNavigator;
using Automation.HOS.Violations;

namespace Automation.HOS.Queries
{
    public class AuditPointQuery : IRequest<ViolationResults>
    {
    public DateTime Timestamp { get; private set; }
    public ITimelineNavigator Navigator { get; private set; }
    public List<AuditRule>? Rules { get; private set; }
    public bool IncludeDebugInfo { get; private set; }


    public AuditPointQuery(DateTime timestamp,
        ITimelineNavigator navigator, List<AuditRule>? rulesToAudit = null, bool includeDebugInfo = false)
    {
        if (navigator == null)
        {
            throw new ArgumentNullException(nameof(navigator));
        }

        Timestamp = timestamp;
        Navigator = navigator;
        Rules = rulesToAudit ?? AuditRules.AllRules;
        IncludeDebugInfo = includeDebugInfo;
    }
    }}
