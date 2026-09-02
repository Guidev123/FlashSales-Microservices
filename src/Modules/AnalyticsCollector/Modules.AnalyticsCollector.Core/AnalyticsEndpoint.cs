using FlashSales.Endpoints.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.AnalyticsCollector.Core.Models;

namespace Modules.AnalyticsCollector.Core
{
    internal sealed class AnalyticsEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/analytics", async (
                AnalyticsRequest request,
                IPublisher publisher,
                CancellationToken cancellationToken) =>
            {
                _ = publisher.PublishToBusAsync(request, cancellationToken);

                return Results.Accepted();
            });
        }
    }
}