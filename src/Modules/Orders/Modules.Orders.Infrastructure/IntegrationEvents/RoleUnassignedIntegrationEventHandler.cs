using FlashSales.Application.Authorization;
using MidR.Interfaces;
using Modules.Users.Contracts.IntegrationEvents;

namespace Modules.Orders.Infrastructure.IntegrationEvents
{
    internal sealed class RoleUnassignedIntegrationEventHandler(IPermissionRepository permissionRepository)
        : INotificationHandler<RoleUnassignedIntegrationEvent>
    {
        public async Task ExecuteAsync(RoleUnassignedIntegrationEvent notification, CancellationToken cancellationToken)
        {
            await permissionRepository.RemoveUserRoleAsync(
                notification.IdentityProviderId, notification.RoleName, cancellationToken);
        }
    }
}
