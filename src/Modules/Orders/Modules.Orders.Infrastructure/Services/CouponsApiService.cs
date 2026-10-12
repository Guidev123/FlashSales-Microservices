using FlashSales.Application.Extensions;
using FlashSales.Domain.Results;
using FlashSales.Infrastructure.Extensions;
using Modules.Coupons.Contracts;
using Modules.Coupons.Contracts.Grpc;
using Modules.Coupons.Contracts.Requests;
using Modules.Coupons.Contracts.Responses;

namespace Modules.Orders.Infrastructure.Services
{
    internal sealed class CouponsApiService(CouponsService.CouponsServiceClient client) : ICouponsPublicApi
    {
        public async Task<Result<RedeemResponse>> RedeemAsync(RedeemRequest request, CancellationToken cancellationToken = default)
        {
            var result = await client.RedeemAsync(new RedeemCouponRequest
            {
                OrderId = request.OrderId.ToString(),
                CustomerId = request.CustomerId.ToString(),
                CouponId = request.CouponId.ToString(),
                OrderAmountCents = request.OrderAmount.ToCents()
            }, cancellationToken: cancellationToken)
            .ExecuteAsync("Coupons.Redeem", cancellationToken);

            return result.Map(_ => new RedeemResponse());
        }

        public async Task<Result> ReleaseAsync(ReleaseRequest request, CancellationToken cancellationToken = default)
        {
            var result = await client.ReleaseAsync(new ReleaseCouponRequest
            {
                CouponId = request.CouponId.ToString(),
                OrderId = request.OrderId.ToString()
            }, cancellationToken: cancellationToken)
            .ExecuteAsync("Coupons.Release", cancellationToken);

            return result;
        }
    }
}