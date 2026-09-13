using FlashSales.Application.Outbox;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FlashSales.Infrastructure.Observability.HealthChecks
{
    internal sealed class OutboxHealthCheck(IOutboxRepository outboxRepository) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var permanentFailures = await outboxRepository.CountPermanentFailuresAsync(cancellationToken);

            return permanentFailures == 0
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Degraded(
                    $"{permanentFailures} outbox message(s) marked as permanent failure and awaiting manual reprocessing.",
                    data: new Dictionary<string, object> { ["permanentFailures"] = permanentFailures });
        }
    }
}
