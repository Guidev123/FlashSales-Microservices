using FlashSales.Application.Abstractions;
using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Coupons.Domain.Coupons.Errors;
using Modules.Coupons.Domain.Coupons.Repositories;

namespace Modules.Coupons.Application.Coupons.Features.Redeem
{
    internal sealed class RedeemCouponCommandHandler(
        ICouponRepository couponRepository,
        IUnitOfWork unitOfWork
        ) : ICommandHandler<RedeemCouponCommand, RedeemCouponResponse>
    {
        private const int MaxRetryAttempts = 3;

        public async Task<Result<RedeemCouponResponse>> ExecuteAsync(RedeemCouponCommand request, CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < MaxRetryAttempts; i++)
            {
                var coupon = await couponRepository.GetByIdAsync(request.CouponId, cancellationToken);
                if (coupon is null)
                {
                    return Result.Failure<RedeemCouponResponse>(CouponErrors.NotFound(request.CouponId));
                }

                var couponRedemptionResult = coupon.Redeem(
                    request.OrderId,
                    request.CustomerId,
                    request.OrderAmount
                    );

                if (couponRedemptionResult.IsFailure)
                {
                    return Result.Failure<RedeemCouponResponse>(couponRedemptionResult.Error!);
                }

                couponRepository.Update(coupon);

                var saveResult = await unitOfWork.SaveChangesAsync(cancellationToken);

                if (saveResult.IsSuccess)
                {
                    var adjustedAmount = couponRedemptionResult.Value - request.OrderAmount;

                    return new RedeemCouponResponse(adjustedAmount);
                }

                if (saveResult.ErrorType != PersistenceErrors.ConcurrencyConflict)
                {
                    return Result.Failure<RedeemCouponResponse>(CouponErrors.SomethingHasFailedToRedeemCoupon);
                }

                if (i < MaxRetryAttempts - 1)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(50 * (i + 1)), cancellationToken);
                }
            }

            return Result.Failure<RedeemCouponResponse>(CouponErrors.InsufficientRedemptionsAvailable);
        }
    }
}