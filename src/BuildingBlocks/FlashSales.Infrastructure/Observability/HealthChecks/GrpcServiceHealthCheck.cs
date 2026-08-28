using Grpc.Health.V1;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FlashSales.Infrastructure.Observability.HealthChecks
{
    internal sealed class GrpcServiceHealthCheck(Health.HealthClient client) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await client.CheckAsync(
                    new HealthCheckRequest(),
                    deadline: DateTime.UtcNow.AddSeconds(5),
                    cancellationToken: cancellationToken);

                return response.Status == HealthCheckResponse.Types.ServingStatus.Serving
                    ? HealthCheckResult.Healthy()
                    : HealthCheckResult.Unhealthy($"Reported status: {response.Status}.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Failed to reach the gRPC health endpoint.", ex);
            }
        }
    }
}
