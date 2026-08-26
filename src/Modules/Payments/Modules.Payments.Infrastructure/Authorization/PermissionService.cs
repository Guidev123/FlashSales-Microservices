using FlashSales.Application.Authorization;
using FlashSales.Application.Cache;
using FlashSales.Domain.DomainObjects;
using FlashSales.Domain.Results;
using FlashSales.Infrastructure.Extensions;
using FlashSales.Users.Contracts.Protos;
using Modules.Users.Contracts.Extensions;

namespace Modules.Payments.Infrastructure.Authorization
{
    internal sealed class PermissionService(
        UserPermissionsService.UserPermissionsServiceClient permissionsServiceClient,
        ICacheService cacheService
        ) : IPermissionService
    {
        public async Task<Result<PermissionResponse>> GetUserPermissionsAsync(string identityId, CancellationToken cancellationToken = default)
        {
            var cachedResult = await cacheService.GetAsync<PermissionResponse>(identityId, cancellationToken);
            if (cachedResult is not null)
            {
                return cachedResult;
            }

            var grpcResult = await permissionsServiceClient.GetUserPermissionsAsync(
                identityId.MapToPermissionRequest(),
                cancellationToken: cancellationToken
                ).ExecuteAsync(nameof(GetUserPermissionsRequest), cancellationToken);

            if (grpcResult.IsFailure)
            {
                throw new FlashSalesException(nameof(GetUserPermissionsRequest), grpcResult.Error!);
            }

            if (!Guid.TryParse(grpcResult.Value.UserId, out var userId))
            {
                throw new FlashSalesException(nameof(GetUserPermissionsRequest), Error.Problem(
                    "Users.InvalidUserId",
                    "The user ID retrieved is not a valid GUID"));
            }

            var result = new PermissionResponse(userId, grpcResult.Value.Permissions.ToHashSet());

            await cacheService.SetAsync(identityId, result, cancellationToken: cancellationToken);

            return result;
        }
    }
}