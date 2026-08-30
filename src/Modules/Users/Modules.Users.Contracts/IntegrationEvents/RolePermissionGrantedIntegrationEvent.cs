using FlashSales.Application.Messaging;

namespace Modules.Users.Contracts.IntegrationEvents
{
    public sealed record RolePermissionGrantedIntegrationEvent : IntegrationEvent
    {
        public static RolePermissionGrantedIntegrationEvent Create(
            Guid correlationId,
            string roleName,
            string permissionCode
            )
        {
            return new(correlationId, roleName, permissionCode);
        }

        private RolePermissionGrantedIntegrationEvent(
            Guid correlationId,
            string roleName,
            string permissionCode
            )
            : base(correlationId, nameof(RolePermissionGrantedIntegrationEvent))
        {
            RoleName = roleName;
            PermissionCode = permissionCode;
        }

        private RolePermissionGrantedIntegrationEvent()
        { }

        public string RoleName { get; set; } = null!;
        public string PermissionCode { get; set; } = null!;
    }
}
