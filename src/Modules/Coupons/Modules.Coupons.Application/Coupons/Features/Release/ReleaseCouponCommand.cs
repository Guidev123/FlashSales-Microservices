using FlashSales.Application.Messaging;

namespace Modules.Coupons.Application.Coupons.Features.Release
{
    public sealed record ReleaseCouponCommand(
        Guid CouponId,
        Guid OrederId
        ) : ICommand;
}