namespace Modules.Coupons.Application.Coupons.DTOs
{
    public sealed record CouponResponse(
        Guid Id,
        string Code,
        string Type,
        decimal Value,
        decimal? MaxValue,
        int AvailableRedemptions,
        DateTimeOffset? ExpirationDate
        );
}