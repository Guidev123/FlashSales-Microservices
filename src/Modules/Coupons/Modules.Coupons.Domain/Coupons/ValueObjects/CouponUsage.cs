using FlashSales.Domain.DomainObjects;
using FlashSales.Domain.ValueObjects;
using Modules.Coupons.Domain.Coupons.Errors;

namespace Modules.Coupons.Domain.Coupons.ValueObjects
{
    public sealed record CouponUsage : ValueObject
    {
        private CouponUsage(int maxRedemptions, int redeemedCount)
        {
            MaxRedemptions = maxRedemptions;
            RedeemedCount = redeemedCount;
            Validate();
        }

        private CouponUsage() { }

        public int MaxRedemptions { get; }
        public int RedeemedCount { get; }
        public int AvailableRedemptions => MaxRedemptions - RedeemedCount;

        public static CouponUsage Create(int maxRedemptions, int redeemedCount = 0) =>
            new(maxRedemptions, redeemedCount);

        protected override void Validate()
        {
            AssertionConcern.EnsureTrue(MaxRedemptions >= 1, CouponErrors.MaxRedemptionsMustBeAtLeastOne.Description);
            AssertionConcern.EnsureTrue(RedeemedCount >= 0, CouponErrors.RedeemedCountCannotBeNegative.Description);
            AssertionConcern.EnsureTrue(RedeemedCount <= MaxRedemptions, CouponErrors.RedeemedCountExceedsMax.Description);
        }
    }
}
