using FlashSales.Domain.DomainObjects;

namespace Modules.Coupons.Domain.Coupons.DomainEvents
{
    public sealed record CouponActivatedDomainEvent : DomainEvent
    {
        public static CouponActivatedDomainEvent Create(Guid couponId)
            => new(couponId);

        private CouponActivatedDomainEvent(Guid couponId)
            : base(couponId, nameof(CouponActivatedDomainEvent))
        {
            CouponId = couponId;
        }

        private CouponActivatedDomainEvent()
        { }

        public Guid CouponId { get; set; }
    }
}
