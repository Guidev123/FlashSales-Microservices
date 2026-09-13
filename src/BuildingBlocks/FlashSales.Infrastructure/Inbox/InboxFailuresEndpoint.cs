using FlashSales.Application.Inbox;
using FlashSales.Endpoints.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FlashSales.Infrastructure.Inbox
{
    internal sealed class InboxFailuresEndpoint(string moduleName) : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var module = moduleName.ToLowerInvariant();

            var group = app.MapGroup($"api/v1/{module}/inbox/failures")
                .WithTags($"{module}-inbox")
                .RequirePermission($"{module}:messaging:reprocess")
                .RequireScope($"{module}.write");

            group.MapGet("", async (
                IInboxRepository inboxRepository,
                CancellationToken cancellationToken) =>
            {
                var failures = await inboxRepository.GetPermanentFailuresAsync(100, cancellationToken);
                return Results.Ok(failures);
            });

            group.MapPost("reprocess", async (
                ReprocessRequest? request,
                IInboxRepository inboxRepository,
                CancellationToken cancellationToken) =>
            {
                var requeued = await inboxRepository.RequeueAsync(request?.CorrelationId, cancellationToken);
                return Results.Ok(new { Requeued = requeued });
            });
        }

        internal sealed record ReprocessRequest(Guid? CorrelationId);
    }
}
