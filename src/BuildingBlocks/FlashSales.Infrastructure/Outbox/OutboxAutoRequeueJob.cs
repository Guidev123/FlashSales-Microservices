using FlashSales.Application.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlashSales.Infrastructure.Outbox
{
    public sealed class OutboxAutoRequeueJob(
        ILogger<OutboxAutoRequeueJob> logger,
        IOptionsMonitor<OutboxOptions> options,
        IServiceProvider serviceProvider,
        string moduleName) : BackgroundService
    {
        private OutboxOptions Options => options.Get(moduleName);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(Options.AutoRequeueIntervalInSeconds));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                if (!Options.AutoRequeueEnabled)
                {
                    continue;
                }

                try
                {
                    await using var scope = serviceProvider.CreateAsyncScope();
                    var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();

                    var requeued = await outboxRepository.RequeueAsync(correlationId: null, stoppingToken);

                    if (requeued > 0)
                    {
                        logger.LogWarning(
                            "[{Module}] Auto-requeued {Count} permanently failed outbox message(s) for another retry cycle",
                            moduleName,
                            requeued);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[{Module}] Unhandled exception in outbox auto-requeue job", moduleName);
                }
            }
        }
    }
}
