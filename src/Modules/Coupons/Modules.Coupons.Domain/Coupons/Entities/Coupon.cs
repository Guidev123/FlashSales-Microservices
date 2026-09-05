using FlashSales.Domain.DomainObjects;
using FlashSales.Domain.Results;
using Modules.Coupons.Domain.Coupons.DomainEvents;
using Modules.Coupons.Domain.Coupons.Enums;
using Modules.Coupons.Domain.Coupons.Errors;
using Modules.Coupons.Domain.Coupons.ValueObjects;

namespace Modules.Coupons.Domain.Coupons.Entities
{
    public sealed class Coupon : Entity, IAggregateRoot
    {
        private readonly List<CouponRedemption> _redemptions = [];

        private Coupon(
            Guid launchId,
            Guid sellerId,
            string code,
            CouponDiscount discount,
            CouponUsage usage,
            CouponValidity validity,
            decimal? minimumOrderAmount,
            int? maxRedemptionsPerCustomer
            )
        {
            LaunchId = launchId;
            SellerId = sellerId;
            Code = code;
            Discount = discount;
            Usage = usage;
            Validity = validity;
            MinimumOrderAmount = minimumOrderAmount;
            MaxRedemptionsPerCustomer = maxRedemptionsPerCustomer;
            Status = CouponStatus.Draft;
            Validate();
        }

        private Coupon()
        { }

        public Guid LaunchId { get; private set; }
        public Guid SellerId { get; private set; }
        public string Code { get; private set; } = null!;
        public CouponDiscount Discount { get; private set; } = null!;
        public CouponUsage Usage { get; private set; } = null!;
        public CouponValidity Validity { get; private set; } = null!;
        public decimal? MinimumOrderAmount { get; private set; }
        public int? MaxRedemptionsPerCustomer { get; private set; }
        public CouponStatus Status { get; private set; }
        public IReadOnlyCollection<CouponRedemption> Redemptions => _redemptions.AsReadOnly();

        internal static Coupon Create(
            Guid launchId,
            Guid sellerId,
            string code,
            CouponDiscount discount,
            CouponUsage usage,
            CouponValidity validity,
            decimal? minimumOrderAmount,
            int? maxRedemptionsPerCustomer
            )
        {
            var coupon = new Coupon(
                launchId,
                sellerId,
                code,
                discount,
                usage,
                validity,
                minimumOrderAmount,
                maxRedemptionsPerCustomer
            );

            coupon.AddDomainEvent(CouponCreatedDomainEvent.Create(coupon.SellerId, coupon.LaunchId, coupon.Id, coupon.Code));

            return coupon;
        }

        public Result<decimal> Redeem(Guid orderId, Guid customerId, decimal orderAmount)
        {
            if (Status != CouponStatus.Active)
            {
                return Result.Failure<decimal>(CouponErrors.InvalidStatusTransition(Status, nameof(Redeem)));
            }

            var alreadyExists = _redemptions.Any(r => r.OrderId == orderId);
            if (alreadyExists)
            {
                return Result.Success(Discount.Apply(orderAmount));
            }

            var currentDate = DateTimeOffset.UtcNow;

            if (!Validity.IsWithin(currentDate))
            {
                return Result.Failure<decimal>(CouponErrors.OutsideValidityWindow(Validity, currentDate));
            }

            if (MinimumOrderAmount is { } min && orderAmount < min)
            {
                return Result.Failure<decimal>(CouponErrors.OrderAmountBelowMinimum(min, orderAmount));
            }

            if (Usage.AvailableRedemptions < 1)
            {
                return Result.Failure<decimal>(CouponErrors.RedemptionLimitReached);
            }

            if (MaxRedemptionsPerCustomer is { } cap && _redemptions.Count(r => r.CustomerId == customerId) >= cap)
            {
                return Result.Failure<decimal>(CouponErrors.CustomerRedemptionLimitReached);
            }

            var discountAmount = Discount.Apply(orderAmount);

            Usage = CouponUsage.Create(Usage.MaxRedemptions, Usage.RedeemedCount + 1);
            _redemptions.Add(CouponRedemption.Create(Id, orderId, customerId));

            AddDomainEvent(CouponRedeemedDomainEvent.Create(Id, orderId, customerId, discountAmount));

            if (Usage.AvailableRedemptions == 0)
            {
                Status = CouponStatus.Exhausted;
                AddDomainEvent(CouponExhaustedDomainEvent.Create(Id));
            }

            return Result.Success(discountAmount);
        }

        public Result Release(Guid orderId)
        {
            if (Status != CouponStatus.Exhausted && Status != CouponStatus.Active)
            {
                return Result.Failure(CouponErrors.InvalidStatusTransition(Status, nameof(Release)));
            }

            var redemption = _redemptions.FirstOrDefault(r => r.OrderId == orderId);
            if (redemption is null)
            {
                return Result.Success();
            }

            _redemptions.Remove(redemption);
            Usage = CouponUsage.Create(Usage.MaxRedemptions, Usage.RedeemedCount - 1);

            if (Status == CouponStatus.Exhausted)
            {
                Status = CouponStatus.Active;
            }

            AddDomainEvent(CouponReleasedDomainEvent.Create(Id, orderId));

            return Result.Success();
        }

        public Result Activate()
        {
            if (Status != CouponStatus.Draft && Status != CouponStatus.Paused)
            {
                return Result.Failure(CouponErrors.InvalidStatusTransition(Status, nameof(Activate)));
            }

            Status = CouponStatus.Active;

            AddDomainEvent(CouponActivatedDomainEvent.Create(Id));

            return Result.Success();
        }

        public Result Pause()
        {
            if (Status != CouponStatus.Active)
            {
                return Result.Failure(CouponErrors.InvalidStatusTransition(Status, nameof(Pause)));
            }

            Status = CouponStatus.Paused;

            AddDomainEvent(CouponPausedDomainEvent.Create(Id));

            return Result.Success();
        }

        public Result Expire()
        {
            if (Status != CouponStatus.Active && Status != CouponStatus.Paused && Status != CouponStatus.Exhausted)
            {
                return Result.Failure(CouponErrors.InvalidStatusTransition(Status, nameof(Expire)));
            }

            Status = CouponStatus.Expired;

            AddDomainEvent(CouponExpiredDomainEvent.Create(Id));

            return Result.Success();
        }

        public Result Cancel()
        {
            if (Status is CouponStatus.Expired or CouponStatus.Canceled)
            {
                return Result.Failure(CouponErrors.InvalidStatusTransition(Status, nameof(Cancel)));
            }

            Status = CouponStatus.Canceled;

            AddDomainEvent(CouponCancelledDomainEvent.Create(Id));

            return Result.Success();
        }

        protected override void Validate()
        {
            AssertionConcern.EnsureTrue(LaunchId != Guid.Empty, CouponErrors.LaunchIdRequired.Description);
            AssertionConcern.EnsureTrue(SellerId != Guid.Empty, CouponErrors.SellerIdRequired.Description);
            AssertionConcern.EnsureNotEmpty(Code, CouponErrors.CodeRequired.Description);
            AssertionConcern.EnsureMaxLength(Code, 50, CouponErrors.CodeTooLong.Description);
            AssertionConcern.EnsureNotNull(Discount, CouponErrors.DiscountCannotBeEmpty.Description);
            AssertionConcern.EnsureNotNull(Usage, CouponErrors.UsageCannotBeEmpty.Description);
            AssertionConcern.EnsureNotNull(Validity, CouponErrors.ValidityCannotBeEmpty.Description);
            AssertionConcern.EnsureTrue(
                MinimumOrderAmount is null || MinimumOrderAmount > 0,
                CouponErrors.InvalidMinimumOrderAmount.Description);
            AssertionConcern.EnsureTrue(
                MaxRedemptionsPerCustomer is null || MaxRedemptionsPerCustomer > 0,
                CouponErrors.InvalidMaxRedemptionsPerCustomer.Description);
        }
    }
}