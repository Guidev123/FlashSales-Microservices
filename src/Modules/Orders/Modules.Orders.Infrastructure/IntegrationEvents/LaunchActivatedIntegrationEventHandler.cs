using MidR.Interfaces;
using Modules.Launches.Contracts.IntegrationEvents;
using Modules.Orders.Application.Launches.Features.Create;

namespace Modules.Orders.Infrastructure.IntegrationEvents
{
    internal sealed class LaunchActivatedIntegrationEventHandler(ISender sender) : INotificationHandler<LaunchActivatedIntegrationEvent>
    {
        public async Task ExecuteAsync(LaunchActivatedIntegrationEvent notification, CancellationToken cancellationToken)
        {
            await sender.SendAsync(new CreateLaunchCommand(
                notification.LaunchId,
                notification.SellerId,
                notification.ProductId,
                notification.Title,
                notification.DiscountedPrice,
                notification.OriginalPrice,
                notification.TotalQuantity,
                notification.StartAt,
                notification.EndAt
                ), cancellationToken);
        }
    }
}
