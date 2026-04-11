using Automation.HOS.Queries;
using Automation.HOS.Violations;

namespace Automation.HOS;

public interface IHosAuditor
{
    Task<ViolationResults> AuditRangeAsync(AuditRangeQuery query);
    Task<ViolationResults> AuditPointAsync(AuditPointQuery query);
    ViolationResults AuditRange(AuditRangeQuery query);
    ViolationResults AuditPoint(AuditPointQuery query);
}