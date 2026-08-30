namespace FlashSales.Infrastructure.Authorization
{
    public sealed class RolePermission
    {
        public string RoleName { get; set; } = null!;
        public string PermissionCode { get; set; } = null!;
    }
}