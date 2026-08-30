using FlashSales.Domain.DomainObjects;

namespace Modules.Users.Domain.Users.DomainEvents
{
    public sealed record RolePermissionGrantedDomainEvent : DomainEvent
    {
        public static RolePermissionGrantedDomainEvent Create(string roleName, string permissionCode)
            => new(roleName, permissionCode);

        private RolePermissionGrantedDomainEvent(string roleName, string permissionCode)
            : base(Guid.Empty, nameof(RolePermissionGrantedDomainEvent))
        {
            RoleName = roleName;
            PermissionCode = permissionCode;
        }

        private RolePermissionGrantedDomainEvent()
        { }

        public string RoleName { get; set; } = null!;
        public string PermissionCode { get; set; } = null!;
    }
}
