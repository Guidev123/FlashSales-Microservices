using FlashSales.Application.Bus;
using MidR.Interfaces;
using Modules.Users.Contracts.IntegrationEvents;
using Modules.Users.Domain.Users.DomainEvents;

namespace Modules.Users.Application.Users.DomainEvents
{
    internal sealed class RolePermissionGrantedDomainEventHandler(IEventBus eventBus)
        : INotificationHandler<RolePermissionGrantedDomainEvent>
    {
        public async Task ExecuteAsync(RolePermissionGrantedDomainEvent notification, CancellationToken cancellationToken)
        {
            var integrationEvent = RolePermissionGrantedIntegrationEvent.Create(
                notification.CorrelationId,
                notification.RoleName,
                notification.PermissionCode);

            await eventBus.PublishAsync(
                Topics.RolePermissionGranted,
                IntegrationEnvelope.FromEvent(integrationEvent),
                cancellationToken);
        }
    }
}
