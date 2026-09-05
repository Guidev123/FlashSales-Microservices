using FlashSales.Domain.DomainObjects;

namespace Modules.Coupons.Domain.Coupons.DomainEvents
{
    public sealed record CouponCancelledDomainEvent : DomainEvent
    {
        public static CouponCancelledDomainEvent Create(Guid couponId)
            => new(couponId);

        private CouponCancelledDomainEvent(Guid couponId)
            : base(couponId, nameof(CouponCancelledDomainEvent))
        {
            CouponId = couponId;
        }

        private CouponCancelledDomainEvent()
        { }

        public Guid CouponId { get; set; }
    }
}
