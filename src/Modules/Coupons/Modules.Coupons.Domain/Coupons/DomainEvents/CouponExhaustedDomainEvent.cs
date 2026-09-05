using FlashSales.Domain.DomainObjects;

namespace Modules.Coupons.Domain.Coupons.DomainEvents
{
    public sealed record CouponExhaustedDomainEvent : DomainEvent
    {
        public static CouponExhaustedDomainEvent Create(Guid couponId)
            => new(couponId);

        private CouponExhaustedDomainEvent(Guid couponId)
            : base(couponId, nameof(CouponExhaustedDomainEvent))
        {
            CouponId = couponId;
        }

        private CouponExhaustedDomainEvent()
        { }

        public Guid CouponId { get; set; }
    }
}
