using FlashSales.Application.Authorization;
using MidR.Interfaces;
using Modules.Users.Contracts.IntegrationEvents;

namespace Modules.Payments.Infrastructure.IntegrationEvents
{
    internal sealed class RolePermissionRevokedIntegrationEventHandler(IPermissionRepository permissionRepository)
        : INotificationHandler<RolePermissionRevokedIntegrationEvent>
    {
        public async Task ExecuteAsync(RolePermissionRevokedIntegrationEvent notification, CancellationToken cancellationToken)
        {
            await permissionRepository.RemoveRolePermissionAsync(
                notification.RoleName, notification.PermissionCode, cancellationToken);
        }
    }
}
