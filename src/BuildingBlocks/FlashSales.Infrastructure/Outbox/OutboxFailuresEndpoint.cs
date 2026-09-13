using FlashSales.Application.Outbox;
using FlashSales.Endpoints.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FlashSales.Infrastructure.Outbox
{
    internal sealed class OutboxFailuresEndpoint(string moduleName) : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var module = moduleName.ToLowerInvariant();

            var group = app.MapGroup($"api/v1/{module}/outbox/failures")
                .WithTags($"{module}-outbox")
                .RequirePermission($"{module}:messaging:reprocess")
                .RequireScope($"{module}.write");

            group.MapGet("", async (
                IOutboxRepository outboxRepository,
                CancellationToken cancellationToken) =>
            {
                var failures = await outboxRepository.GetPermanentFailuresAsync(100, cancellationToken);
                return Results.Ok(failures);
            });

            group.MapPost("reprocess", async (
                ReprocessRequest? request,
                IOutboxRepository outboxRepository,
                CancellationToken cancellationToken) =>
            {
                var requeued = await outboxRepository.RequeueAsync(request?.CorrelationId, cancellationToken);
                return Results.Ok(new { Requeued = requeued });
            });
        }

        internal sealed record ReprocessRequest(Guid? CorrelationId);
    }
}
