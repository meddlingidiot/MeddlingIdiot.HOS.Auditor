using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace MeddlingIdiot.HOS.TimelineNavigator.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    public static class TimelineNavigatorDependencyInjectionExtensions
    {
        public static IServiceCollection AddTimelineNavigator(
            this IServiceCollection services)
        {
            services.AddTransient<ITimelineNavigator, TimelineNavigator>();

            return services;
        }
    }
}