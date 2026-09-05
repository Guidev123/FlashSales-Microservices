using FlashSales.Domain.ValueObjects;

namespace Modules.Coupons.Domain.Coupons.ValueObjects
{
    public sealed record CouponValidity : ValueObject
    {
        public DateTimeOffset ValidFrom { get; }
        public DateTimeOffset ValidUntil { get; }

        internal bool IsWithin(DateTimeOffset currentDate)
        {
            return true;
        }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
