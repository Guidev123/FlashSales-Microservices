using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FlashSales.Infrastructure.Observability.HealthChecks
{
    internal sealed class ServiceBusClientHealthCheck(ServiceBusClient client) : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var result = client.IsClosed
                ? HealthCheckResult.Unhealthy("Service Bus client is closed.")
                : HealthCheckResult.Healthy();

            return Task.FromResult(result);
        }
    }
}
