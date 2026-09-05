using FlashSales.Domain.DomainObjects;
using Modules.Coupons.Domain.Coupons.Enums;
using Modules.Coupons.Domain.Coupons.ValueObjects;

namespace Modules.Coupons.Domain.Coupons.Entities
{
    public sealed class Coupon : Entity, IAggregateRoot
    {
        private readonly List<CouponRedemption> _redemptions = [];

        public Guid LaunchId { get; private set; }
        public Guid SellerId { get; private set; }
        public string Code { get; private set; } = null!;
        public CouponDiscount Discount { get; private set; } = null!;
        public CouponUsage Usage { get; private set; } = null!;
        public CouponValidity Validity { get; private set; } = null!;
        public decimal? MinimumOrderAmount { get; private set; }
        public decimal? MaxRedemptionsPerCustomer { get; private set; }
        public CouponStatus Status { get; private set; }
        public IReadOnlyCollection<CouponRedemption> Redemptions => _redemptions.AsReadOnly();
        protected override void Validate()
        {
        }
    }
}
