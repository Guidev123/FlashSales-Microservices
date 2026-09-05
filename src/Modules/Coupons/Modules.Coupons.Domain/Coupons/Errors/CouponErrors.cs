using FlashSales.Domain.Results;
using Modules.Coupons.Domain.Coupons.Enums;
using Modules.Coupons.Domain.Coupons.ValueObjects;

namespace Modules.Coupons.Domain.Coupons.Errors
{
    public static class CouponErrors
    {
        public static Error NotFound(Guid id) => Error.NotFound(
            "Coupons.NotFound",
            $"Coupon with id {id} was not found");

        public static Error InvalidStatusTransition(CouponStatus current, string attempted)
            => Error.Problem(
                "Coupons.InvalidStatusTransition",
                $"Cannot transition from '{current}' to '{attempted}'");

        public static readonly Error LaunchIdRequired = Error.Invalid(
            "Coupons.LaunchIdRequired",
            "Launch id must not be empty");

        public static readonly Error SellerIdRequired = Error.Invalid(
            "Coupons.SellerIdRequired",
            "Seller id must not be empty");

        public static readonly Error CodeRequired = Error.Invalid(
            "Coupons.CodeRequired",
            "Code must not be empty");

        public static readonly Error CodeTooLong = Error.Invalid(
            "Coupons.CodeTooLong",
            "Code must be at most 50 characters long");

        public static readonly Error DiscountCannotBeEmpty = Error.Invalid(
            "Coupons.DiscountCannotBeEmpty",
            "Discount can not be empty");

        public static readonly Error UsageCannotBeEmpty = Error.Invalid(
            "Coupons.UsageCannotBeEmpty",
            "Usage can not be empty");

        public static readonly Error ValidityCannotBeEmpty = Error.Invalid(
            "Coupons.ValidityCannotBeEmpty",
            "Validity can not be empty");

        public static readonly Error InvalidMinimumOrderAmount = Error.Invalid(
            "Coupons.InvalidMinimumOrderAmount",
            "Minimum order amount must be greater than zero when informed");

        public static readonly Error InvalidMaxRedemptionsPerCustomer = Error.Invalid(
            "Coupons.InvalidMaxRedemptionsPerCustomer",
            "Max redemptions per customer must be greater than zero when informed");

        public static readonly Error DiscountTypeRequired = Error.Invalid(
            "Coupons.DiscountTypeRequired",
            "Discount type must be either Percentage or Fixed");

        public static readonly Error InvalidPercentageValue = Error.Invalid(
            "Coupons.InvalidPercentageValue",
            "Percentage discount value must be between 1 and 100");

        public static readonly Error InvalidFixedDiscountValue = Error.Invalid(
            "Coupons.InvalidFixedDiscountValue",
            "Fixed discount value must be greater than zero");

        public static readonly Error MaxDiscountAmountOnlyForPercentage = Error.Invalid(
            "Coupons.MaxDiscountAmountOnlyForPercentage",
            "Max discount amount can only be set for percentage discounts");

        public static readonly Error InvalidMaxDiscountAmount = Error.Invalid(
            "Coupons.InvalidMaxDiscountAmount",
            "Max discount amount must be greater than zero when informed");

        public static readonly Error MaxRedemptionsMustBeAtLeastOne = Error.Invalid(
            "Coupons.MaxRedemptionsMustBeAtLeastOne",
            "Max redemptions must be at least 1");

        public static readonly Error RedeemedCountCannotBeNegative = Error.Invalid(
            "Coupons.RedeemedCountCannotBeNegative",
            "Redeemed count must be zero or greater");

        public static readonly Error RedeemedCountExceedsMax = Error.Invalid(
            "Coupons.RedeemedCountExceedsMax",
            "Redeemed count cannot exceed max redemptions");

        public static readonly Error InvalidValidityWindow = Error.Invalid(
            "Coupons.InvalidValidityWindow",
            "ValidFrom must be before ValidUntil");

        public static readonly Error CouponIdRequired = Error.Invalid(
            "Coupons.CouponIdRequired",
            "Coupon id must not be empty");

        public static readonly Error OrderIdRequired = Error.Invalid(
            "Coupons.OrderIdRequired",
            "Order id must not be empty");

        public static readonly Error CustomerIdRequired = Error.Invalid(
            "Coupons.CustomerIdRequired",
            "Customer id must not be empty");

        public static Error OutsideValidityWindow(
            CouponValidity validity,
            DateTimeOffset currentDate)
            => Error.Problem(
                "Coupons.OutsideValidityWindow",
                $"The coupon is outside its validity period. " +
                $"Valid from {validity.ValidFrom} until {validity.ValidUntil}.");

        public static Error OrderAmountBelowMinimum(decimal minimumOrderAmount, decimal orderAmount)
            => Error.Problem(
                "Coupons.OrderAmountBelowMinimum",
                $"Order amount {orderAmount} is below the minimum required amount of {minimumOrderAmount} to use this coupon.");

        public static readonly Error RedemptionLimitReached = Error.Problem(
            "Coupons.RedemptionLimitReached",
            "This coupon has reached its maximum number of redemptions.");

        public static readonly Error CustomerRedemptionLimitReached = Error.Problem(
            "Coupons.CustomerRedemptionLimitReached",
            "This customer has already reached the maximum number of redemptions allowed for this coupon.");
    }
}