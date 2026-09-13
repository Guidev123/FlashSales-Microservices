using FlashSales.Application.Inbox;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FlashSales.Infrastructure.Observability.HealthChecks
{
    internal sealed class InboxHealthCheck(IInboxRepository inboxRepository) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var permanentFailures = await inboxRepository.CountPermanentFailuresAsync(cancellationToken);

            return permanentFailures == 0
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Degraded(
                    $"{permanentFailures} inbox message(s) marked as permanent failure and awaiting manual reprocessing.",
                    data: new Dictionary<string, object> { ["permanentFailures"] = permanentFailures });
        }
    }
}
