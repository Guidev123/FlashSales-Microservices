using FlashSales.Application.Messaging;

namespace Modules.Coupons.Application.Coupons.Features.Redeem
{
    public sealed record RedeemCouponCommand(
        Guid OrderId,
        Guid CustomerId,
        Guid CouponId,
        decimal OrderAmount
        ) : ICommand, ITransactionalLessCommand;
}