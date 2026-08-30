namespace FlashSales.Application.Authorization
{
    public interface IPermissionRepository
    {
        Task UpsertUserRoleAsync(string identityProviderId, Guid userId, string roleName, CancellationToken cancellationToken = default);

        Task RemoveUserRoleAsync(string identityProviderId, string roleName, CancellationToken cancellationToken = default);

        Task UpsertRolePermissionAsync(string roleName, string permissionCode, CancellationToken cancellationToken = default);

        Task RemoveRolePermissionAsync(string roleName, string permissionCode, CancellationToken cancellationToken = default);
    }
}