using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Users.Domain.AccessManagement.Errors;
using Modules.Users.Domain.AccessManagement.Repositories;
using Modules.Users.Domain.Users.DomainEvents;

namespace Modules.Users.Application.AccessManagement.Features.RevokePermission
{
    internal sealed class RevokePermissionCommandHandler(
        IRoleRepository roleRepository,
        IDomainEventCollector domainEventCollector) : ICommandHandler<RevokePermissionCommand>
    {
        public async Task<Result> ExecuteAsync(RevokePermissionCommand request, CancellationToken cancellationToken = default)
        {
            var roleExists = await roleRepository.RoleExistsAsync(request.RoleName, cancellationToken);
            if (!roleExists)
            {
                return Result.Failure(AccessManagementErrors.RoleNotFound(request.RoleName));
            }

            await roleRepository.RevokePermissionAsync(request.RoleName, request.PermissionCode, cancellationToken);

            domainEventCollector.Collect(RolePermissionRevokedDomainEvent.Create(request.RoleName, request.PermissionCode));

            return Result.Success();
        }
    }
}