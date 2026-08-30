using MidR.Interfaces;
using Modules.Launches.Contracts.IntegrationEvents;
using Modules.Orders.Application.Launches.Features.Cancel;

namespace Modules.Orders.Infrastructure.IntegrationEvents
{
    internal sealed class LaunchCancelledIntegrationEventHandler(ISender sender) : INotificationHandler<LaunchCancelledIntegrationEvent>
    {
        public async Task ExecuteAsync(LaunchCancelledIntegrationEvent notification, CancellationToken cancellationToken)
        {
            await sender.SendAsync(new CancelLaunchCommand(notification.LaunchId), cancellationToken);
        }
    }
}
