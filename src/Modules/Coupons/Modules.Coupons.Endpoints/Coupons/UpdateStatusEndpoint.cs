using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Coupons.Application.Coupons.Features.UpdateStatus;
using Modules.Coupons.Domain.Coupons.Enums;

namespace Modules.Coupons.Endpoints.Coupons
{
    internal sealed class UpdateStatusEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPatch("api/v1/coupons", async (
                ISender sender,
                Request request,
                CancellationToken cancellationToken
                ) =>
            {
                var result = await sender.SendAsync(new UpdateCouponStatusCommand(
                    request.CouponId,
                    request.Status), cancellationToken);

                return result.Match(Results.NoContent, ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
            .RequirePermission(CouponsPermissions.UpdateStatus)
            .RequireScope(CouponsScopes.Write);
        }

        sealed record Request(
            Guid CouponId,
            CouponStatus Status
            );
    }
}