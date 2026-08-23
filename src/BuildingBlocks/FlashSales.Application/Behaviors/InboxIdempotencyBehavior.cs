using FlashSales.Application.Inbox;
using FlashSales.Application.Messaging;
using MidR.Behaviors;

namespace FlashSales.Application.Behaviors
{
    public sealed class InboxIdempotencyBehavior<TNotification>(
           IInboxRepository inboxRepository
           )
           : INotificationBehavior<TNotification>
           where TNotification : IntegrationEvent
    {
        public async Task ExecuteAsync(TNotification notification, NotificationDelegate next, CancellationToken cancellationToken)
        {
            var isProcessed = await inboxRepository.IsProcessedAsync(notification.CorrelationId, notification.MessageType, cancellationToken);
            if (isProcessed)
            {
                return;
            }

            await next();

            await inboxRepository.MarkAsProcessedAsync(notification.CorrelationId, notification.MessageType, cancellationToken);
        }
    }
}
