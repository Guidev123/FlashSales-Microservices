using Microsoft.AspNetCore.Authorization;

namespace FlashSales.Infrastructure.Authorization
{
    internal sealed class ScopeRequirement : IAuthorizationRequirement
    {
        public ScopeRequirement(IReadOnlyCollection<string> scopes) => Scopes = scopes;

        public IReadOnlyCollection<string> Scopes { get; }
    }
}
