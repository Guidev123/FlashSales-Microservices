using FlashSales.Application.Extensions;
using FluentValidation;
using Modules.Coupons.Domain.Coupons.Errors;

namespace Modules.Coupons.Application.Coupons.Features.Release
{
    internal sealed class ReleaseCouponCommandValidator : AbstractValidator<ReleaseCouponCommand>
    {
        public ReleaseCouponCommandValidator()
        {
            RuleFor(c => c.CouponId)
                .NotEqual(Guid.Empty)
                .WithError(CouponErrors.CouponIdRequired);
        }
    }
}