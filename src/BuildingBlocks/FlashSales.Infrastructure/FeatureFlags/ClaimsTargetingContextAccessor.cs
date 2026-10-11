using Microsoft.AspNetCore.Http;
using Microsoft.FeatureManagement.FeatureFilters;
using System.Security.Claims;
using System.Text.Json;

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
                Groups = GetGroups(user)
            };

            return ValueTask.FromResult(context);
        }

        private static List<string> GetGroups(ClaimsPrincipal user)
        {
            var groups = user.FindAll("role").Concat(user.FindAll(ClaimTypes.Role)).Select(c => c.Value).ToList();

            var realmAccess = user.FindFirst("realm_access")?.Value;

            if (!string.IsNullOrWhiteSpace(realmAccess))
            {
                try
                {
                    using var document = JsonDocument.Parse(realmAccess);

                    if (document.RootElement.TryGetProperty("roles", out var roles) && roles.ValueKind == JsonValueKind.Array)
                        groups.AddRange(roles.EnumerateArray()
                            .Where(role => role.ValueKind == JsonValueKind.String)
                            .Select(role => role.GetString()!));
                }
                catch (JsonException)
                {
                }
            }

            return [.. groups.Distinct()];
        }
    }
}