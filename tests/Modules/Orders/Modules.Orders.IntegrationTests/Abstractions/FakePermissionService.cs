using FlashSales.Application.Authorization;
using FlashSales.Domain.Results;

namespace Modules.Orders.IntegrationTests.Abstractions
{
    internal sealed class FakePermissionService : IPermissionService
    {
        private readonly HashSet<string> _permissions = [];

        public Guid UserId { get; set; } = Guid.NewGuid();

        public void Grant(params string[] permissions) => _permissions.UnionWith(permissions);

        public void Reset()
        {
            UserId = Guid.NewGuid();
            _permissions.Clear();
        }

        public Task<Result<PermissionResponse>> GetUserPermissionsAsync(string identityId, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success(new PermissionResponse(UserId, [.. _permissions])));
    }
}
