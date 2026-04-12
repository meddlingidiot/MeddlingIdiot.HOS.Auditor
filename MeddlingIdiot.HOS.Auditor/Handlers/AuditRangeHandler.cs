using System.Diagnostics.CodeAnalysis;
using MeddlingIdiot.Dispatcher;
using MeddlingIdiot.HOS.Queries;
using MeddlingIdiot.HOS.Violations;

namespace MeddlingIdiot.HOS.Handlers
{
    [ExcludeFromCodeCoverage]
    internal class AuditRangeHandler : IRequestHandler<AuditRangeQuery, ViolationResults>
    {
        private readonly IHosAuditor _hosAuditor;

        public AuditRangeHandler(IHosAuditor hosAuditor)
        {
            _hosAuditor = hosAuditor;
        }

        public Task<ViolationResults> Handle(AuditRangeQuery request, CancellationToken cancellationToken)
        {
            return _hosAuditor.AuditRangeAsync( request);
        }
    }
}
