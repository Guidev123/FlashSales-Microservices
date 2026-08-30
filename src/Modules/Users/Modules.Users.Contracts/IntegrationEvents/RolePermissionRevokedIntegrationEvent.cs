using FlashSales.Application.Messaging;

namespace Modules.Users.Contracts.IntegrationEvents
{
    public sealed record RolePermissionRevokedIntegrationEvent : IntegrationEvent
    {
        public static RolePermissionRevokedIntegrationEvent Create(
            Guid correlationId,
            string roleName,
            string permissionCode
            )
        {
            return new(correlationId, roleName, permissionCode);
        }

        private RolePermissionRevokedIntegrationEvent(
            Guid correlationId,
            string roleName,
            string permissionCode
            )
            : base(correlationId, nameof(RolePermissionRevokedIntegrationEvent))
        {
            RoleName = roleName;
            PermissionCode = permissionCode;
        }

        private RolePermissionRevokedIntegrationEvent()
        { }

        public string RoleName { get; set; } = null!;
        public string PermissionCode { get; set; } = null!;
    }
}
