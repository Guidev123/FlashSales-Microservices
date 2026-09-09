using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Coupons.Domain.Coupons.Builders;
using Modules.Coupons.Domain.Coupons.Repositories;
using Modules.Coupons.Domain.Coupons.ValueObjects;

namespace Modules.Coupons.Application.Coupons.Features.Create
{
    internal sealed class CreateCouponCommandHandler(
        ICouponRepository couponRepository
        ) : ICommandHandler<CreateCouponCommand>
    {
        public async Task<Result> ExecuteAsync(CreateCouponCommand request, CancellationToken cancellationToken = default)
        {
            var exists = await couponRepository.ExistsAsync(request.Code, cancellationToken);
            if (exists)
            {
                return Result.Success();
            }

            var coupon = new CouponBuilder(request.LaunchId, request.SellerId, request.Code)
                .WithDiscount(request.DiscountAmount, request.DiscountPercentage, request.MaxDiscountAmount)
                .WithMinimumOrderAmount(request.MinimumOrderAmount)
                .WithMaxRedemptionsPerCustomer(request.MaxRedemptionsPerCustomer)
                .WithValidity(CouponValidity.Create(request.ValidFrom, request.ValidUntil))
                .WithUsage(request.MaxRedemptions)
                .Build();

            couponRepository.Add(coupon);

            return Result.Success();
        }
    }
}