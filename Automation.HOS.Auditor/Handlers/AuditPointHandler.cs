using System.Diagnostics.CodeAnalysis;
using Automation.Dispatcher;
using Automation.HOS.Queries;
using Automation.HOS.Violations;

namespace Automation.HOS.Handlers
{
    [ExcludeFromCodeCoverage]
    internal class AuditPointHandler : IRequestHandler<AuditPointQuery, ViolationResults>
    {
        private readonly IHosAuditor _hosAuditor;

        public AuditPointHandler(IHosAuditor hosAuditor)
        {
            _hosAuditor = hosAuditor;
        }

        public Task<ViolationResults> Handle(AuditPointQuery request, CancellationToken cancellationToken)
        {
            return _hosAuditor.AuditPointAsync(request);
        }
    }
}
