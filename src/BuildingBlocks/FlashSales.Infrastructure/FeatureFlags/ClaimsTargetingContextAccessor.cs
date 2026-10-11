using Microsoft.AspNetCore.Http;
using Microsoft.FeatureManagement.FeatureFilters;
using System.Security.Claims;

namespace FlashSales.Infrastructure.FeatureFlags
{
    internal sealed class ClaimsTargetingContextAccessor(IHttpContextAccessor httpContextAccessor) : ITargetingContextAccessor
    {
        public ValueTask<TargetingContext> GetContextAsync()
        {
            var user = httpContextAccessor.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated != true)
                return ValueTask.FromResult(new TargetingContext());

            var context = new TargetingContext
            {
                UserId = user.FindFirst("sub")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Groups = [.. user.FindAll("role").Concat(user.FindAll(ClaimTypes.Role)).Select(c => c.Value).Distinct()]
            };

            return ValueTask.FromResult(context);
        }
    }
}
