using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Launches.Application.Launches.Features.ReleaseStock;

namespace Modules.Launches.Endpoints.Launches
{
    internal sealed class ReleaseStockEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/launches/stock/release", async (
                ReleaseStockRequest request,
                ISender sender,
                CancellationToken cancellationToken
                ) =>
            {
                var result = await sender.SendAsync(new ReleaseStockCommand(
                    request.LaunchId,
                    request.OrderId,
                    request.Quantity
                    ), cancellationToken);

                return result.Match(Results.NoContent, ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
            .RequireScope(LaunchesScopes.StockManagement);
        }

        record ReleaseStockRequest(
            Guid LaunchId,
            int Quantity,
            Guid OrderId
            );
    }
}