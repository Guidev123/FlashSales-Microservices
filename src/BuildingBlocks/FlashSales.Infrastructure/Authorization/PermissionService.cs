using Dapper;
using FlashSales.Application.Abstractions;
using FlashSales.Application.Authorization;
using FlashSales.Domain.Results;
using FlashSales.Infrastructure.Extensions;

namespace FlashSales.Infrastructure.Authorization
{
    internal sealed class PermissionService(IUnitOfWork unitOfWork, string schema) : IPermissionService
    {
        public async Task<Result<PermissionResponse>> GetUserPermissionsAsync(string identityId, CancellationToken cancellationToken = default)
        {
            var sql = $"""
            SELECT ur."UserId", rp."PermissionCode"
            FROM {schema}."UserRoles" ur
            LEFT JOIN {schema}."RolePermissions" rp ON rp."RoleName" = ur."RoleName"
            WHERE ur."IdentityProviderId" = @IdentityProviderId
            """;

            var rows = await unitOfWork.Connection.QueryAsync<(Guid UserId, string? PermissionCode)>(
                unitOfWork.CreateCommand(sql, new { IdentityProviderId = identityId }, cancellationToken));

            var list = rows.ToList();
            if (list.Count == 0)
                return new PermissionResponse(Guid.Empty, []);

            var permissions = list
                .Where(r => r.PermissionCode is not null)
                .Select(r => r.PermissionCode!)
                .ToHashSet();

            return new PermissionResponse(list[0].UserId, permissions);
        }
    }
}