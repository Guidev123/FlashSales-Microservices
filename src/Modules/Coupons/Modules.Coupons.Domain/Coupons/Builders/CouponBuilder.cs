using FlashSales.Domain.DomainObjects;
using Modules.Coupons.Domain.Coupons.Entities;
using Modules.Coupons.Domain.Coupons.Enums;
using Modules.Coupons.Domain.Coupons.Errors;
using Modules.Coupons.Domain.Coupons.ValueObjects;

namespace Modules.Coupons.Domain.Coupons.Builders
{
    public sealed class CouponBuilder
    {
        private readonly Guid _launchId;
        private readonly Guid _sellerId;
        private readonly string _code;
        private CouponUsage? _usage;
        private CouponValidity? _validity;
        private CouponDiscount? _discount;
        private decimal? _minimumOrderAmount = null;
        private int? _maxRedemptionsPerCustomer = null;

        public CouponBuilder(Guid launchId, Guid sellerId, string code)
        {
            _launchId = launchId;
            _sellerId = sellerId;
            _code = code;
        }

        public CouponBuilder WithUsage(int maxRedemptions)
        {
            _usage = CouponUsage.Create(maxRedemptions);
            return this;
        }

        public CouponBuilder WithValidity(CouponValidity validity)
        {
            _validity = validity;
            return this;
        }

        public CouponBuilder WithMinimumOrderAmount(decimal? amount)
        {
            if (!amount.HasValue) return this;
            _minimumOrderAmount = amount.Value;
            return this;
        }

        public CouponBuilder WithMaxRedemptionsPerCustomer(int? quantity)
        {
            if (!quantity.HasValue) return this;
            _maxRedemptionsPerCustomer = quantity.Value;
            return this;
        }

        public CouponBuilder WithoutMinimumOrderAmount()
        {
            _minimumOrderAmount = null;
            return this;
        }

        public CouponBuilder WithoutRedemptionPerCustomerLimit()
        {
            _maxRedemptionsPerCustomer = null;
            return this;
        }

        public CouponBuilder WithDiscount(
            decimal? discountAmount,
            decimal? discountPercentage,
            decimal? maxDiscountAmount
            )
        {
            if (discountAmount is null && discountPercentage is not null)
            {
                _discount = CouponDiscount.CreatePercentage(discountPercentage.Value, maxDiscountAmount);
                return this;
            }

            if (discountPercentage is null && discountAmount is not null)
            {
                _discount = CouponDiscount.CreateFixed(discountAmount.Value, maxDiscountAmount);
                return this;
            }

            throw new DomainException(CouponErrors.InvalidDiscountParameters.Description);
        }

        public Coupon Build()
        {
            if (_discount is null)
                throw new DomainException(CouponErrors.DiscountCannotBeEmpty.Description);

            if (_usage is null)
                throw new DomainException(CouponErrors.UsageCannotBeEmpty.Description);

            if (_validity is null)
                throw new DomainException(CouponErrors.ValidityCannotBeEmpty.Description);

            return Coupon.Create(
                _launchId,
                _sellerId,
                _code,
                _discount,
                _usage,
                _validity,
                _minimumOrderAmount,
                _maxRedemptionsPerCustomer
                );
        }
    }
}