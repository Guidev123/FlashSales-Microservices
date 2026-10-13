using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Coupons.Domain.Coupons.Repositories;

namespace Modules.Coupons.Application.Coupons.Features.Release
{
    internal sealed class ReleaseCouponCommandHandler(
        ICouponRepository couponRepository
        ) : ICommandHandler<ReleaseCouponCommand>
    {
        public async Task<Result> ExecuteAsync(ReleaseCouponCommand request, CancellationToken cancellationToken = default)
        {
            var coupon = await couponRepository.GetByIdAsync(request.CouponId, cancellationToken);
            if (coupon is null)
            {
                return Result.Success();
            }

            var releaseResult = coupon.Release(request.OrederId);
            if (releaseResult.IsFailure)
            {
                return releaseResult;
            }

            couponRepository.Update(coupon);

            return Result.Success();
        }
    }
}