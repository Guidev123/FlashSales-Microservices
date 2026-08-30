using FlashSales.Application.Authorization;
using MidR.Interfaces;
using Modules.Users.Contracts.IntegrationEvents;

namespace Modules.Orders.Infrastructure.IntegrationEvents
{
    internal sealed class RoleAssignedIntegrationEventHandler(IPermissionRepository permissionRepository)
        : INotificationHandler<RoleAssignedIntegrationEvent>
    {
        public async Task ExecuteAsync(RoleAssignedIntegrationEvent notification, CancellationToken cancellationToken)
        {
            await permissionRepository.UpsertUserRoleAsync(
                notification.IdentityProviderId, notification.UserId, notification.RoleName, cancellationToken);
        }
    }
}
