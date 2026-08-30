using FlashSales.Application.Bus;
using FlashSales.Domain.DomainObjects;
using MidR.Interfaces;
using Modules.Users.Contracts.IntegrationEvents;
using Modules.Users.Domain.Users.DomainEvents;
using Modules.Users.Domain.Users.Errors;
using Modules.Users.Domain.Users.Repositories;

namespace Modules.Users.Application.Users.DomainEvents
{
    internal sealed class RoleAssignedToUserDomainEventHandler(
        IUserRepository userRepository,
        IEventBus eventBus
        ) : INotificationHandler<RoleAssignedToUserDomainEvent>
    {
        public async Task ExecuteAsync(RoleAssignedToUserDomainEvent notification, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetAsync(notification.UserId, cancellationToken);
            if (user is null)
            {
                throw new FlashSalesException(nameof(RoleAssignedToUserDomainEvent), UserErrors.NotFound(notification.UserId));
            }

            var integrationEvent = RoleAssignedIntegrationEvent.Create(
                notification.CorrelationId,
                notification.UserId,
                user.IdentiyProviderId,
                notification.Role);

            await eventBus.PublishAsync(
                Topics.RoleAssigned,
                IntegrationEnvelope.FromEvent(integrationEvent),
                cancellationToken);
        }
    }
}
