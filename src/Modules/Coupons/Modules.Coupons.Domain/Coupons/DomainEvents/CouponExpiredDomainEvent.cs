using FlashSales.Domain.DomainObjects;

namespace Modules.Coupons.Domain.Coupons.DomainEvents
{
    public sealed record CouponExpiredDomainEvent : DomainEvent
    {
        public static CouponExpiredDomainEvent Create(Guid couponId)
            => new(couponId);

        private CouponExpiredDomainEvent(Guid couponId)
            : base(couponId, nameof(CouponExpiredDomainEvent))
        {
            CouponId = couponId;
        }

        private CouponExpiredDomainEvent()
        { }

        public Guid CouponId { get; set; }
    }
}
