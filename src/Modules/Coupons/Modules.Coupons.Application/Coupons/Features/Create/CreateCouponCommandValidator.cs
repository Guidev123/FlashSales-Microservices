using FluentValidation;

namespace Modules.Coupons.Application.Coupons.Features.Create
{
    internal sealed class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
    {
        public CreateCouponCommandValidator()
        {
            RuleFor(x => x.LaunchId).NotEmpty();
            RuleFor(x => x.SellerId).NotEmpty();
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.MaxRedemptions).GreaterThan(0);
            RuleFor(x => x.ValidFrom).LessThan(x => x.ValidUntil);
            RuleFor(x => x.MinimumOrderAmount).GreaterThanOrEqualTo(0).When(x => x.MinimumOrderAmount.HasValue);
            RuleFor(x => x.MaxRedemptionsPerCustomer).GreaterThan(0).When(x => x.MaxRedemptionsPerCustomer.HasValue);
            RuleFor(x => x.DiscountAmount).GreaterThan(0).When(x => x.DiscountAmount.HasValue);
            RuleFor(x => x.DiscountPercentage).InclusiveBetween(1, 100).When(x => x.DiscountPercentage.HasValue);
            RuleFor(x => x.MaxDiscountAmount).GreaterThanOrEqualTo(0).When(x => x.MaxDiscountAmount.HasValue);
        }
    }
}