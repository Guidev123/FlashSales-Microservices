using FlashSales.Domain.DomainObjects;

namespace Modules.Users.Domain.Users.DomainEvents
{
    public sealed record RolePermissionRevokedDomainEvent : DomainEvent
    {
        public static RolePermissionRevokedDomainEvent Create(string roleName, string permissionCode)
            => new(roleName, permissionCode);

        private RolePermissionRevokedDomainEvent(string roleName, string permissionCode)
            : base(Guid.Empty, nameof(RolePermissionRevokedDomainEvent))
        {
            RoleName = roleName;
            PermissionCode = permissionCode;
        }

        private RolePermissionRevokedDomainEvent()
        { }

        public string RoleName { get; set; } = null!;
        public string PermissionCode { get; set; } = null!;
    }
}
