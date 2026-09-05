using FlashSales.Domain.DomainObjects;
using Modules.Coupons.Domain.Coupons.Entities;
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

        public CouponBuilder WithUsage(CouponUsage usage)
        {
            _usage = usage;
            return this;
        }

        public CouponBuilder WithValidity(CouponValidity validity)
        {
            _validity = validity;
            return this;
        }

        public CouponBuilder WithMinimumOrderAmount(decimal amount)
        {
            _minimumOrderAmount = amount;
            return this;
        }

        public CouponBuilder WithMaxRedemptionsPerCustomer(int quantity)
        {
            _maxRedemptionsPerCustomer = quantity;
            return this;
        }

        public CouponBuilder WithoutMinimumOrderAmount()
        {
            _minimumOrderAmount = null;
            return this;
        }

        public CouponBuilder WithoutRedemptionLimit()
        {
            _maxRedemptionsPerCustomer = null;
            return this;
        }

        public CouponBuilder WithDiscount(CouponDiscount discount)
        {
            _discount = discount;
            return this;
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