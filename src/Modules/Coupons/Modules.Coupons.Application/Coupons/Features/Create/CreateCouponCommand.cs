using FlashSales.Application.Messaging;

namespace Modules.Coupons.Application.Coupons.Features.Create
{
    public sealed record CreateCouponCommand(
        Guid LaunchId,
        Guid SellerId,
        string Code,
        int MaxRedemptions,
        DateTimeOffset ValidFrom,
        DateTimeOffset ValidUntil,
        decimal? MinimumOrderAmount,
        int? MaxRedemptionsPerCustomer,
        decimal? DiscountAmount,
        int? DiscountPercentage,
        decimal? MaxDiscountAmount
        ) : ICommand;
}