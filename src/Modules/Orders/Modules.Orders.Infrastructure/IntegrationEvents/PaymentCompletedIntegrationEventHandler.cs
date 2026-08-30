using MidR.Interfaces;
using Modules.Orders.Application.Orders.Features.Confirm;
using Modules.Payments.Contracts.IntegrationEvents;

namespace Modules.Orders.Infrastructure.IntegrationEvents
{
    internal sealed class PaymentCompletedIntegrationEventHandler(ISender sender) : INotificationHandler<PaymentCompletedIntegrationEvent>
    {
        public async Task ExecuteAsync(PaymentCompletedIntegrationEvent notification, CancellationToken cancellationToken)
        {
            await sender.SendAsync(new ConfirmOrderCommand(notification.OrderId), cancellationToken);
        }
    }
}
