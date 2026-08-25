using FlashSales.Application.Authorization;
using FlashSales.Application.Cache;
using FlashSales.Domain.Results;
using FlashSales.Users.Contracts.Protos;

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
        }
    }
}
