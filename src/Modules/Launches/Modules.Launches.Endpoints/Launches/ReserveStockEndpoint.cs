using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Launches.Application.Launches.Features.ReserveStock;

namespace Modules.Launches.Endpoints.Launches
{
    internal sealed class ReserveStockEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/launches/{launchId:guid}/stock/reserve", async (
                ReserveStockRequest request,
                Guid launchId,
                ISender sender,
                CancellationToken cancellationToken
                ) =>
            {
                var result = await sender.SendAsync(new ReserveStockCommand(
                    launchId,
                    request.OrderId,
                    request.Quantity
                    ), cancellationToken);
                return result.Match(Results.NoContent, ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
            .RequireScope(LaunchesScopes.StockManagement);
        }

        record ReserveStockRequest(
            int Quantity,
            Guid OrderId
            );
    }
}