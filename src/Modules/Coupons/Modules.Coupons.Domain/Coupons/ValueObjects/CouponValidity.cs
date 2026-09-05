using FlashSales.Domain.DomainObjects;
using FlashSales.Domain.ValueObjects;
using Modules.Coupons.Domain.Coupons.Errors;

namespace Modules.Coupons.Domain.Coupons.ValueObjects
{
    public sealed record CouponValidity : ValueObject
    {
        private CouponValidity(DateTimeOffset validFrom, DateTimeOffset validUntil)
        {
            ValidFrom = validFrom;
            ValidUntil = validUntil;
            Validate();
        }

        private CouponValidity() { }

        public DateTimeOffset ValidFrom { get; }
        public DateTimeOffset ValidUntil { get; }

        public static CouponValidity Create(DateTimeOffset validFrom, DateTimeOffset validUntil) =>
            new(validFrom, validUntil);

        internal bool IsWithin(DateTimeOffset currentDate) =>
            currentDate >= ValidFrom && currentDate <= ValidUntil;

        protected override void Validate()
        {
            AssertionConcern.EnsureTrue(ValidFrom < ValidUntil, CouponErrors.InvalidValidityWindow.Description);
        }
    }
}
