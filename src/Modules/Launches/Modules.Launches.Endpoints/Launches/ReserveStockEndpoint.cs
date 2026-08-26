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
            app.MapPost("api/v1/launches/stock/reserve", async (
                ReserveStockRequest request,
                ISender sender,
                CancellationToken cancellationToken
                ) =>
            {
                var result = await sender.SendAsync(new ReserveStockCommand(
                    request.LaunchId,
                    request.OrderId,
                    request.Quantity
                    ), cancellationToken);
                return result.Match(Results.NoContent, ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
            .RequireScope(LaunchesScopes.StockManagement);
        }

        record ReserveStockRequest(
            Guid LaunchId,
            int Quantity,
            Guid OrderId
            );
    }
}