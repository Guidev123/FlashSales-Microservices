using FlashSales.Domain.DomainObjects;

namespace Modules.Coupons.Domain.Coupons.DomainEvents
{
    public sealed record CouponReleasedDomainEvent : DomainEvent
    {
        public static CouponReleasedDomainEvent Create(Guid couponId, Guid orderId)
            => new(couponId, orderId);

        private CouponReleasedDomainEvent(Guid couponId, Guid orderId)
            : base(couponId, nameof(CouponReleasedDomainEvent))
        {
            CouponId = couponId;
            OrderId = orderId;
        }

        private CouponReleasedDomainEvent()
        { }

        public Guid CouponId { get; set; }
        public Guid OrderId { get; set; }
    }
}
