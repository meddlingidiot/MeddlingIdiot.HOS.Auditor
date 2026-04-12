using System.Diagnostics.CodeAnalysis;
using MeddlingIdiot.Dispatcher;
using MeddlingIdiot.HOS.Queries;
using MeddlingIdiot.HOS.Ruleset;
using MeddlingIdiot.HOS.TimelineNavigator.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace MeddlingIdiot.HOS.DependencyInjection
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
