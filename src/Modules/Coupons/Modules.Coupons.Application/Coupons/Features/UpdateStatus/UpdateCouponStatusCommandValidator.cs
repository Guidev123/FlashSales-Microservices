using FlashSales.Application.Extensions;
using FluentValidation;
using Modules.Coupons.Domain.Coupons.Enums;
using Modules.Coupons.Domain.Coupons.Errors;

namespace Modules.Coupons.Application.Coupons.Features.UpdateStatus
{
    internal sealed class UpdateCouponStatusCommandValidator : AbstractValidator<UpdateCouponStatusCommand>
    {
        public UpdateCouponStatusCommandValidator()
        {
            RuleFor(x => x.CouponId)
                .NotEmpty()
                .WithError(CouponErrors.CouponIdRequired);

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithError(CouponErrors.InvalidStatus)
                .NotEqual(CouponStatus.None)
                .WithError(CouponErrors.InvalidStatus);
        }
    }
}