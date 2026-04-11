using System.Diagnostics.CodeAnalysis;
using Automation.Dispatcher;
using Automation.HOS.Queries;
using Automation.HOS.Ruleset;
using Automation.HOS.TimelineNavigator.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Automation.HOS.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    public static class AuditorDependencyInjectionExtensions
    {
        public static IServiceCollection AddAuditor(
            this IServiceCollection services)
        {
            services.AddSingleton<IHosAuditor>(new HosAuditor(new Us60HrRuleDefinition()));
            services.AddSingleton<IGpsStreamToDutyStatusTimeline>(new GpsStreamToDutyStatusTimeline(new InMemoryLogger()));

            services.AddDispatcher(typeof(AuditRangeQuery).Assembly);
            return services;
        }
    }
}
