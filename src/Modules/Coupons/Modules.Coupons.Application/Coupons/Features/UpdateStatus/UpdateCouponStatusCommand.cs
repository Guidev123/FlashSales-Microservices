using FlashSales.Application.Messaging;
using Modules.Coupons.Domain.Coupons.Enums;

namespace Modules.Coupons.Application.Coupons.Features.UpdateStatus
{
    public sealed record UpdateCouponStatusCommand(
        Guid CouponId,
        CouponStatus Status
        ) : ICommand;
}