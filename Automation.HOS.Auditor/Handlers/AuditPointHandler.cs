using Automation.Dispatcher;
using Automation.HOS.Queries;
using Automation.HOS.Violations;

namespace Automation.HOS.Handlers
{
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
