using FlashSales.Application.Authorization;
using Microsoft.AspNetCore.Builder;

namespace FlashSales.Endpoints.Endpoints
{
    public static class AuthorizationEndpointExtensions
    {
        public static TBuilder RequireScope<TBuilder>(this TBuilder builder, params string[] scopes)
            where TBuilder : IEndpointConventionBuilder
        {
            return builder.RequireAuthorization(AuthorizationPolicies.ForScopes(scopes));
        }
    }
}
