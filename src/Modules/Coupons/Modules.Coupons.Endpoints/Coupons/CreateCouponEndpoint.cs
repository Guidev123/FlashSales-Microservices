using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Coupons.Application.Coupons.Features.Create;

namespace Modules.Coupons.Endpoints.Coupons
{
    internal sealed class CreateCouponEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/coupons", async (
                CreateCouponCommand request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);

                return result.Match(Results.Created, ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
            .RequirePermission(CouponsPermissions.Create)
            .RequireScope(CouponsScopes.Write);
        }
    }
}