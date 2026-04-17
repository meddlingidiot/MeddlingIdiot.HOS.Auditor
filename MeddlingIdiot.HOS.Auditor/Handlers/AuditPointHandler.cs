using System.Diagnostics.CodeAnalysis;
using MeddlingIdiot.Dispatcher;
using MeddlingIdiot.HOS.Queries;
using MeddlingIdiot.HOS.Violations;

namespace MeddlingIdiot.HOS.Handlers
{
    [ExcludeFromCodeCoverage]
    internal class AuditPointHandler : IRequestHandler<AuditPointQuery, ViolationResults>
    {
        private readonly IHosAuditor _hosAuditor;

        public AuditPointHandler(IHosAuditor hosAuditor)
        {
            _hosAuditor = hosAuditor;
        }

        public Task<ViolationResults> Handle(AuditPointQuery request, CancellationToken cancellationToken = default)
        {
            return _hosAuditor.AuditPointAsync(request, cancellationToken);
        }
    }
}
