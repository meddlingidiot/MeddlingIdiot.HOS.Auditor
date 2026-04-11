using System.Diagnostics.CodeAnalysis;
using Automation.Dispatcher;
using Automation.HOS.Queries;
using Automation.HOS.Violations;

namespace Automation.HOS.Handlers
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
