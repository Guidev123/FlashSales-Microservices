using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Coupons.Domain.Coupons.Errors;
using Modules.Coupons.Domain.Coupons.Repositories;

namespace Modules.Coupons.Application.Coupons.Features.UpdateStatus
{
    internal sealed class UpdateCouponStatusCommandHandler(
        ICouponRepository couponRepository
        ) : ICommandHandler<UpdateCouponStatusCommand>
    {
        public async Task<Result> ExecuteAsync(UpdateCouponStatusCommand request, CancellationToken cancellationToken = default)
        {
            var coupon = await couponRepository.GetByIdAsync(request.CouponId, cancellationToken);
            if (coupon is null)
            {
                return Result.Failure(CouponErrors.NotFound(request.CouponId));
            }

            var statusResult = coupon.UpdateStatus(request.Status);
            if (statusResult.IsFailure)
            {
                return statusResult;
            }

            couponRepository.Update(coupon);

            return Result.Success();
        }
    }
}