using MeddlingIdiot.HOS.Queries;
using MeddlingIdiot.HOS.Violations;

namespace MeddlingIdiot.HOS;

public interface IHosAuditor
{
    Task<ViolationResults> AuditRangeAsync(AuditRangeQuery query);
    Task<ViolationResults> AuditPointAsync(AuditPointQuery query);
    ViolationResults AuditRange(AuditRangeQuery query);
    ViolationResults AuditPoint(AuditPointQuery query);
}