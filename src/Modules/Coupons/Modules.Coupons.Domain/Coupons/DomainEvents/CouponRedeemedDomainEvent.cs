using FlashSales.Domain.DomainObjects;

namespace Modules.Coupons.Domain.Coupons.DomainEvents
{
    public sealed record CouponRedeemedDomainEvent : DomainEvent
    {
        public static CouponRedeemedDomainEvent Create(Guid couponId, Guid orderId, Guid customerId, decimal discountAmount)
            => new(couponId, orderId, customerId, discountAmount);

        private CouponRedeemedDomainEvent(Guid couponId, Guid orderId, Guid customerId, decimal discountAmount)
            : base(couponId, nameof(CouponRedeemedDomainEvent))
        {
            CouponId = couponId;
            OrderId = orderId;
            CustomerId = customerId;
            DiscountAmount = discountAmount;
        }

        private CouponRedeemedDomainEvent()
        { }

        public Guid CouponId { get; set; }
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}
