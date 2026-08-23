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
            app.MapPost("api/v1/launches/{launchId:guid}/stock/release", async (
                ReleaseStockRequest request,
                Guid launchId,
                ISender sender,
                CancellationToken cancellationToken
                ) =>
            {
                var result = await sender.SendAsync(new ReleaseStockCommand(
                    launchId,
                    request.OrderId,
                    request.Quantity
                    ), cancellationToken);

                return result.Match(Results.NoContent, ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
            .RequireAuthorization();
        }

        record ReleaseStockRequest(
            int Quantity,
            Guid OrderId
            );
    }
}