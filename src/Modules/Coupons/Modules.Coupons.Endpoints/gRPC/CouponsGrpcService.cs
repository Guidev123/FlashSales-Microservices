using FlashSales.Application.Extensions;
using FlashSales.Infrastructure.Extensions;
using Grpc.Core;
using MidR.Interfaces;
using Modules.Coupons.Application.Coupons.Features.Redeem;
using Modules.Coupons.Application.Coupons.Features.Release;
using Modules.Coupons.Contracts.Grpc;

namespace Modules.Coupons.Endpoints.gRPC
{
    internal sealed class CouponsGrpcService(ISender sender) : CouponsService.CouponsServiceBase
    {
        public override async Task<RedeemCouponReply> Redeem(RedeemCouponRequest request, ServerCallContext context)
        {
            var result = await sender.SendAsync(new RedeemCouponCommand(
                request.OrderId.ToGuid(nameof(request.OrderId)),
                request.CustomerId.ToGuid(nameof(request.CustomerId)),
                request.CouponId.ToGuid(nameof(request.CouponId)),
                request.OrderAmountCents.ToAmount()
                ), context.CancellationToken);

            result.ThrowIfFailure();

            return new();
        }

        public override async Task<ReleaseCouponReply> Release(ReleaseCouponRequest request, ServerCallContext context)
        {
            var result = await sender.SendAsync(new ReleaseCouponCommand(
                request.CouponId.ToGuid(nameof(request.CouponId)),
                request.OrderId.ToGuid(nameof(request.OrderId))
                ), context.CancellationToken);

            result.ThrowIfFailure();

            return new();
        }
    }
}
