using FlashSales.Application.Authorization;
using MidR.Interfaces;
using Modules.Users.Contracts.IntegrationEvents;

namespace Modules.Orders.Infrastructure.IntegrationEvents
{
    internal sealed class RolePermissionGrantedIntegrationEventHandler(IPermissionRepository permissionRepository)
        : INotificationHandler<RolePermissionGrantedIntegrationEvent>
    {
        public async Task ExecuteAsync(RolePermissionGrantedIntegrationEvent notification, CancellationToken cancellationToken)
        {
            await permissionRepository.UpsertRolePermissionAsync(
                notification.RoleName, notification.PermissionCode, cancellationToken);
        }
    }
}
