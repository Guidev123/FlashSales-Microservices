namespace Modules.Coupons.Contracts.Requests
{
    public sealed record RedeemRequest(
        Guid OrderId,
        Guid CustomerId,
        Guid CouponId,
        decimal OrderAmount
        );
}