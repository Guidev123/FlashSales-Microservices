using FlashSales.Domain.DomainObjects;
using Modules.Coupons.Domain.Coupons.Errors;

namespace Modules.Coupons.Domain.Coupons.Entities
{
    public sealed class CouponRedemption : Entity
    {
        private CouponRedemption(Guid couponId, Guid orderId, Guid customerId)
        {
            CouponId = couponId;
            OrderId = orderId;
            CustomerId = customerId;
            Validate();
        }

        private CouponRedemption()
        { }

        public Guid CouponId { get; private set; }
        public Guid OrderId { get; private set; }
        public Guid CustomerId { get; private set; }

        public static CouponRedemption Create(Guid couponId, Guid orderId, Guid customerId)
        {
            var couponRedemption = new CouponRedemption(couponId, orderId, customerId);

            return couponRedemption;
        }

        protected override void Validate()
        {
            AssertionConcern.EnsureTrue(CouponId != Guid.Empty, CouponErrors.CouponIdRequired.Description);
            AssertionConcern.EnsureTrue(OrderId != Guid.Empty, CouponErrors.OrderIdRequired.Description);
            AssertionConcern.EnsureTrue(CustomerId != Guid.Empty, CouponErrors.CustomerIdRequired.Description);
        }
    }
}
