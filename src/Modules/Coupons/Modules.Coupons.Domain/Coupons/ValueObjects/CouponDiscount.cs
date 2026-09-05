using FlashSales.Domain.DomainObjects;
using FlashSales.Domain.ValueObjects;
using Modules.Coupons.Domain.Coupons.Enums;
using Modules.Coupons.Domain.Coupons.Errors;

namespace Modules.Coupons.Domain.Coupons.ValueObjects
{
    public sealed record CouponDiscount : ValueObject
    {
        private CouponDiscount(CouponType type, decimal value, decimal? maxDiscountAmount)
        {
            Type = type;
            Value = value;
            MaxDiscountAmount = maxDiscountAmount;
            Validate();
        }

        private CouponDiscount() { }

        public CouponType Type { get; }
        public decimal Value { get; }
        public decimal? MaxDiscountAmount { get; }

        public static CouponDiscount Create(CouponType type, decimal value, decimal? maxDiscountAmount = null) =>
            new(type, value, maxDiscountAmount);

        internal decimal Apply(decimal orderAmount)
        {
            var discount = Type == CouponType.Percentage
                ? orderAmount * (Value / 100m)
                : Value;

            if (MaxDiscountAmount is { } cap)
                discount = Math.Min(discount, cap);

            return Math.Min(discount, orderAmount);
        }

        protected override void Validate()
        {
            AssertionConcern.EnsureFalse(Type == CouponType.None, CouponErrors.DiscountTypeRequired.Description);

            if (Type == CouponType.Percentage)
                AssertionConcern.EnsureInRange(Value, 1m, 100m, CouponErrors.InvalidPercentageValue.Description);
            else
                AssertionConcern.EnsureGreaterThan(Value, 0m, CouponErrors.InvalidFixedDiscountValue.Description);

            AssertionConcern.EnsureTrue(
                MaxDiscountAmount is null || Type == CouponType.Percentage,
                CouponErrors.MaxDiscountAmountOnlyForPercentage.Description);

            if (MaxDiscountAmount is { } cap)
                AssertionConcern.EnsureGreaterThan(cap, 0m, CouponErrors.InvalidMaxDiscountAmount.Description);
        }
    }
}
