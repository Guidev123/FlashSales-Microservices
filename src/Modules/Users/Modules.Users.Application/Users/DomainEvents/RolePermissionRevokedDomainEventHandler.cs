using FlashSales.Application.Bus;
using MidR.Interfaces;
using Modules.Users.Contracts.IntegrationEvents;
using Modules.Users.Domain.Users.DomainEvents;

namespace Modules.Users.Application.Users.DomainEvents
{
    internal sealed class RolePermissionRevokedDomainEventHandler(IEventBus eventBus)
        : INotificationHandler<RolePermissionRevokedDomainEvent>
    {
        public async Task ExecuteAsync(RolePermissionRevokedDomainEvent notification, CancellationToken cancellationToken)
        {
            var integrationEvent = RolePermissionRevokedIntegrationEvent.Create(
                notification.CorrelationId,
                notification.RoleName,
                notification.PermissionCode);

            await eventBus.PublishAsync(
                Topics.RolePermissionRevoked,
                IntegrationEnvelope.FromEvent(integrationEvent),
                cancellationToken);
        }
    }
}
