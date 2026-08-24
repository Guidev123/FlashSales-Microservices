using FlashSales.Application.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace FlashSales.Infrastructure.Authorization
{
    internal sealed class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        private readonly ConcurrentDictionary<string, AuthorizationPolicy> _policyCache = new();

        public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
            : base(options) { }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            var explicitPolicy = await base.GetPolicyAsync(policyName);
            if (explicitPolicy is not null)
                return explicitPolicy;

            if (_policyCache.TryGetValue(policyName, out var cachedPolicy))
                return cachedPolicy;

            var policy = policyName.StartsWith(AuthorizationPolicies.ScopePolicyPrefix, StringComparison.Ordinal)
                ? BuildScopePolicy(policyName)
                : BuildPermissionPolicy(policyName);

            _policyCache[policyName] = policy;

            return policy;
        }

        private static AuthorizationPolicy BuildScopePolicy(string policyName)
        {
            var scopes = policyName[AuthorizationPolicies.ScopePolicyPrefix.Length..]
                .Split(',', StringSplitOptions.RemoveEmptyEntries);

            return new AuthorizationPolicyBuilder()
                .AddRequirements(new ScopeRequirement(scopes))
                .Build();
        }

        private static AuthorizationPolicy BuildPermissionPolicy(string policyName)
        {
            return new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();
        }
    }
}