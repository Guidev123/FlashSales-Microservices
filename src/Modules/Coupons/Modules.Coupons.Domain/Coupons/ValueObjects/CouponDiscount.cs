using FlashSales.Domain.ValueObjects;
using Modules.Coupons.Domain.Coupons.Enums;

namespace Modules.Coupons.Domain.Coupons.ValueObjects
{
    public sealed record CouponDiscount : ValueObject
    {
        public CouponType Type { get; }
        public decimal Value { get; }
        public decimal? MaxDiscountAmount { get; }


        internal decimal Apply(decimal orderAmount)
        {
            return decimal.Zero;
        }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
