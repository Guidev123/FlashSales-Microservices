using FlashSales.Domain.DomainObjects;

namespace Modules.Coupons.Domain.Coupons.DomainEvents
{
    public sealed record CouponPausedDomainEvent : DomainEvent
    {
        public static CouponPausedDomainEvent Create(Guid couponId)
            => new(couponId);

        private CouponPausedDomainEvent(Guid couponId)
            : base(couponId, nameof(CouponPausedDomainEvent))
        {
            CouponId = couponId;
        }

        private CouponPausedDomainEvent()
        { }

        public Guid CouponId { get; set; }
    }
}
