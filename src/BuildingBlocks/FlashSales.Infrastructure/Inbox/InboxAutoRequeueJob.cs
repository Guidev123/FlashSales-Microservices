using FlashSales.Application.Inbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlashSales.Infrastructure.Inbox
{
    public sealed class InboxAutoRequeueJob(
        ILogger<InboxAutoRequeueJob> logger,
        IOptionsMonitor<InboxOptions> options,
        IServiceProvider serviceProvider,
        string moduleName) : BackgroundService
    {
        private InboxOptions Options => options.Get(moduleName);

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
                    var inboxRepository = scope.ServiceProvider.GetRequiredService<IInboxRepository>();

                    var requeued = await inboxRepository.RequeueAsync(correlationId: null, stoppingToken);

                    if (requeued > 0)
                    {
                        logger.LogWarning(
                            "[{Module}] Auto-requeued {Count} permanently failed inbox message(s) for another retry cycle",
                            moduleName,
                            requeued);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[{Module}] Unhandled exception in inbox auto-requeue job", moduleName);
                }
            }
        }
    }
}
