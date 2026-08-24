using Microsoft.AspNetCore.Authorization;

namespace FlashSales.Infrastructure.Authorization
{
    internal sealed class ScopeAuthorizationHandler : AuthorizationHandler<ScopeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScopeRequirement requirement)
        {
            var grantedScopes = context.User
                .FindAll(c => c.Type is "scope" or "scp")
                .SelectMany(c => c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .ToHashSet(StringComparer.Ordinal);

            if (requirement.Scopes.All(grantedScopes.Contains))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
