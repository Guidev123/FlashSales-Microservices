using FlashSales.Domain.DomainObjects;

namespace Modules.Coupons.Domain.Coupons.DomainEvents
{
    public sealed record CouponCreatedDomainEvent : DomainEvent
    {
        public static CouponCreatedDomainEvent Create(Guid sellerId, Guid launchId, Guid couponId, string code)
            => new(sellerId, launchId, couponId, code);

        private CouponCreatedDomainEvent(Guid sellerId, Guid launchId, Guid couponId, string code)
            : base(couponId, nameof(CouponCreatedDomainEvent))
        {
            SellerId = sellerId;
            LaunchId = launchId;
            CouponId = couponId;
            Code = code;
        }

        private CouponCreatedDomainEvent()
        { }

        public Guid SellerId { get; set; }
        public Guid LaunchId { get; set; }
        public Guid CouponId { get; set; }
        public string Code { get; set; } = null!;
    }
}
