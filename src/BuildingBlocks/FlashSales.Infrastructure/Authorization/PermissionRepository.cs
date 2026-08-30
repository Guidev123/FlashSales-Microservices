using Dapper;
using FlashSales.Application.Abstractions;
using FlashSales.Application.Authorization;
using FlashSales.Infrastructure.Extensions;

namespace FlashSales.Infrastructure.Authorization
{
    internal sealed class PermissionRepository(IUnitOfWork unitOfWork, string schema) : IPermissionRepository
    {
        public Task UpsertUserRoleAsync(string identityProviderId, Guid userId, string roleName, CancellationToken cancellationToken = default)
        {
            var sql = $"""
            INSERT INTO {schema}."UserRoles" ("IdentityProviderId", "UserId", "RoleName")
            VALUES (@IdentityProviderId, @UserId, @RoleName)
            ON CONFLICT ("IdentityProviderId", "RoleName") DO UPDATE SET "UserId" = EXCLUDED."UserId"
            """;

            return unitOfWork.Connection.ExecuteAsync(unitOfWork.CreateCommand(sql, new
            {
                IdentityProviderId = identityProviderId,
                UserId = userId,
                RoleName = roleName
            }, cancellationToken));
        }

        public Task RemoveUserRoleAsync(string identityProviderId, string roleName, CancellationToken cancellationToken = default)
        {
            var sql = $"""
            DELETE FROM {schema}."UserRoles"
            WHERE "IdentityProviderId" = @IdentityProviderId
              AND "RoleName" = @RoleName
            """;

            return unitOfWork.Connection.ExecuteAsync(unitOfWork.CreateCommand(sql, new
            {
                IdentityProviderId = identityProviderId,
                RoleName = roleName
            }, cancellationToken));
        }

        public Task UpsertRolePermissionAsync(string roleName, string permissionCode, CancellationToken cancellationToken = default)
        {
            var sql = $"""
            INSERT INTO {schema}."RolePermissions" ("RoleName", "PermissionCode")
            VALUES (@RoleName, @PermissionCode)
            ON CONFLICT DO NOTHING
            """;

            return unitOfWork.Connection.ExecuteAsync(unitOfWork.CreateCommand(sql, new
            {
                RoleName = roleName,
                PermissionCode = permissionCode
            }, cancellationToken));
        }

        public Task RemoveRolePermissionAsync(string roleName, string permissionCode, CancellationToken cancellationToken = default)
        {
            var sql = $"""
            DELETE FROM {schema}."RolePermissions"
            WHERE "RoleName" = @RoleName
              AND "PermissionCode" = @PermissionCode
            """;

            return unitOfWork.Connection.ExecuteAsync(unitOfWork.CreateCommand(sql, new
            {
                RoleName = roleName,
                PermissionCode = permissionCode
            }, cancellationToken));
        }
    }
}