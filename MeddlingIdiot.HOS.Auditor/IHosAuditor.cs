using MeddlingIdiot.HOS.Queries;
using MeddlingIdiot.HOS.Violations;

namespace MeddlingIdiot.HOS;

public interface IHosAuditor
{
    Task<ViolationResults> AuditRangeAsync(AuditRangeQuery query, CancellationToken cancellationToken);
    Task<ViolationResults> AuditPointAsync(AuditPointQuery query, CancellationToken cancellationToken);
    ViolationResults AuditRange(AuditRangeQuery query, CancellationToken cancellationToken = default);
    ViolationResults AuditPoint(AuditPointQuery query, CancellationToken cancellationToken = default);
}