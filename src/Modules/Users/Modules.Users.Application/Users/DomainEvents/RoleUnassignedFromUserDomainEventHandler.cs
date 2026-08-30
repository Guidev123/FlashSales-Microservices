using FlashSales.Application.Bus;
using FlashSales.Domain.DomainObjects;
using MidR.Interfaces;
using Modules.Users.Contracts.IntegrationEvents;
using Modules.Users.Domain.Users.DomainEvents;
using Modules.Users.Domain.Users.Errors;
using Modules.Users.Domain.Users.Repositories;

namespace Modules.Users.Application.Users.DomainEvents
{
    internal sealed class RoleUnassignedFromUserDomainEventHandler(
        IUserRepository userRepository,
        IEventBus eventBus
        ) : INotificationHandler<RoleUnassignedFromUserDomainEvent>
    {
        public async Task ExecuteAsync(RoleUnassignedFromUserDomainEvent notification, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetAsync(notification.UserId, cancellationToken);
            if (user is null)
            {
                throw new FlashSalesException(nameof(RoleUnassignedFromUserDomainEvent), UserErrors.NotFound(notification.UserId));
            }

            var integrationEvent = RoleUnassignedIntegrationEvent.Create(
                notification.CorrelationId,
                user.IdentiyProviderId,
                notification.Role);

            await eventBus.PublishAsync(
                Topics.RoleUnassigned,
                IntegrationEnvelope.FromEvent(integrationEvent),
                cancellationToken);
        }
    }
}
