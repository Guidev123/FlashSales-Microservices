using FlashSales.Domain.DomainObjects;

namespace Modules.Coupons.Domain.Coupons.Entities
{
    public sealed class CouponRedemption : Entity
    {
        public Guid CouponId { get; private set; }
        public Guid OrderId { get; private set; }
        public Guid CustomerId { get; private set; }

        protected override void Validate()
        {
        }
    }
}
