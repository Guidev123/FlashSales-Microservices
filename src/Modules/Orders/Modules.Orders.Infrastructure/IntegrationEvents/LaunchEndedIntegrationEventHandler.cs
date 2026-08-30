using MidR.Interfaces;
using Modules.Launches.Contracts.IntegrationEvents;
using Modules.Orders.Application.Launches.Features.End;

namespace Modules.Orders.Infrastructure.IntegrationEvents
{
    internal sealed class LaunchEndedIntegrationEventHandler(ISender sender) : INotificationHandler<LaunchEndedIntegrationEvent>
    {
        public async Task ExecuteAsync(LaunchEndedIntegrationEvent notification, CancellationToken cancellationToken)
        {
            await sender.SendAsync(new EndLaunchCommand(notification.LaunchId), cancellationToken);
        }
    }
}
