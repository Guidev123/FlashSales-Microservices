using FlashSales.Domain.ValueObjects;

namespace Modules.Coupons.Domain.Coupons.ValueObjects
{
    public sealed record CouponUsage : ValueObject
    {
        public int MaxRedemptions { get; }
        public int RedeemedCount { get; }
        public int AvailableRedemptions => MaxRedemptions - RedeemedCount;

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
