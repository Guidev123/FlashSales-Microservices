using FluentValidation;
using Modules.Coupons.Domain.Coupons.Errors;

namespace Modules.Coupons.Application.Coupons.Features.Redeem
{
    internal sealed class RedeemCouponCommandValidator : AbstractValidator<RedeemCouponCommand>
    {
        public RedeemCouponCommandValidator()
        {
            RuleFor(c => c.CustomerId)
                .NotEqual(Guid.Empty)
                .WithMessage(CouponErrors.CustomerIdRequired.Description)
                .NotNull();

            RuleFor(c => c.OrderId)
                .NotEqual(Guid.Empty)
                .WithMessage(CouponErrors.OrderIdRequired.Description)
                .NotNull();

            RuleFor(c => c.CouponId)
                .NotEqual(Guid.Empty)
                .WithMessage(CouponErrors.CouponIdRequired.Description)
                .NotNull();
        }
    }
}