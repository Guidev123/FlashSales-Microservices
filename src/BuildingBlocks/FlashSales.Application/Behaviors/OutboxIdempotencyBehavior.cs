using FlashSales.Application.Outbox;
using FlashSales.Domain.DomainObjects;
using MidR.Behaviors;

namespace FlashSales.Application.Behaviors
{
    public sealed class OutboxIdempotencyBehavior<TNotification>(
           IOutboxRepository outboxRepository
           )
           : INotificationBehavior<TNotification>
           where TNotification : DomainEvent
    {
        public async Task ExecuteAsync(TNotification notification, NotificationDelegate next, CancellationToken cancellationToken)
        {
            var isProcessed = await outboxRepository.IsProcessedAsync(notification.CorrelationId, notification.MessageType, cancellationToken);
            if (isProcessed)
            {
                return;
            }

            await next();

            await outboxRepository.MarkAsProcessedAsync(notification.CorrelationId, notification.MessageType, cancellationToken);
        }
    }
}