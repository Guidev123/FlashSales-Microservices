namespace Modules.Coupons.Contracts.Requests
{
    public sealed record ReleaseRequest(
        Guid CouponId,
        Guid OrderId
        );
}