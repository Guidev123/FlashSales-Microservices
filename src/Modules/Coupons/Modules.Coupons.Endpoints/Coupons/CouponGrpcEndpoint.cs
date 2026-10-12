using FlashSales.Endpoints.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Modules.Coupons.Endpoints.gRPC;

namespace Modules.Coupons.Endpoints.Coupons
{
    internal sealed class CouponGrpcEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGrpcService<CouponsGrpcService>()
                .RequireScope(CouponsScopes.Write);
        }
    }
}