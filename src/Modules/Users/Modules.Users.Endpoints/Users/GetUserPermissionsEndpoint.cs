using FlashSales.Users.Contracts.Protos;
using Grpc.Core;
using MidR.Interfaces;
using Modules.Users.Application.AccessManagement.Features.GetPermissions;

namespace Modules.Users.Endpoints.Users
{
    public sealed class GetUserPermissionsEndpoint(ISender sender) : UserPermissionsService.UserPermissionsServiceBase
    {
        public override async Task<FlashSales.Users.Contracts.Protos.GetUserPermissionsResponse> GetUserPermissions(GetUserPermissionsRequest request, ServerCallContext context)
        {
            var result = await sender.SendAsync(new GetUserPermissionsQuery(request.IdentityId), context.CancellationToken);
            if (result.IsFailure)
            {
                throw new RpcException(new Status(StatusCode.NotFound, result.Error!.Description));
            }

            var response = new FlashSales.Users.Contracts.Protos.GetUserPermissionsResponse { UserId = result.Value.UserId.ToString() };

            var permissions = result.Value.Roles
                .SelectMany(x => x.Permissions)
                .ToHashSet();

            response.Permissions.AddRange(permissions);

            return response;
        }
    }
}