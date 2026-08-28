using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System.Security.Claims;

namespace FlashSales.Infrastructure.Middlewares
{
    internal sealed class CallerLoggingMiddleware : IMiddleware
    {
        private const string ServiceAccountPrefix = "service-account-";

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.User.Identity is not { IsAuthenticated: true })
            {
                await next(context);
                return;
            }

            var preferredUsername = context.User.FindFirst("preferred_username")?.Value;

            if (preferredUsername is not null && preferredUsername.StartsWith(ServiceAccountPrefix, StringComparison.Ordinal))
            {
                var clientId = context.User.FindFirst("azp")?.Value ?? context.User.FindFirst("client_id")?.Value ?? preferredUsername;

                using (LogContext.PushProperty("CallerType", "Service"))
                using (LogContext.PushProperty("CallerId", clientId))
                {
                    await next(context);
                }

                return;
            }

            var userId = context.User.FindFirst("sub")?.Value ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            using (LogContext.PushProperty("CallerType", "User"))
            using (LogContext.PushProperty("CallerId", userId))
            {
                await next(context);
            }
        }
    }
}
