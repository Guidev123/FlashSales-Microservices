using Microsoft.Extensions.DependencyInjection;

namespace Modules.AnalyticsCollector.Core
{
    public static class AnalyticsCollectorModule
    {
        public static IServiceCollection AddAnalyticsCollectorModule(this IServiceCollection services)
        {
            return services;
        }
    }
}