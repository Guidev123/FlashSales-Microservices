using FlashSales.Domain.Results;

namespace Modules.Orders.Domain.Launches.Errors
{
    public static class LaunchErrors
    {
        public static readonly Error LaunchIdRequired = Error.Invalid(
            "Launches.LaunchIdRequired",
            "Launch id must not be empty");

        public static readonly Error SellerIdRequired = Error.Invalid(
            "Launches.SellerIdRequired",
            "Seller id must not be empty");

        public static readonly Error ProductIdRequired = Error.Invalid(
            "Launches.ProductIdRequired",
            "Product id must not be empty");

        public static readonly Error TitleRequired = Error.Invalid(
            "Launches.TitleRequired",
            "Title must not be empty");

        public static Error FailToReleaseStockForOrder(Guid orderId, Guid launchId)
            => Error.Conflict(
                "Launches.FailToReleaseStockForOrder",
                $"Fail to release stock for order {orderId} and launch {launchId}");

        public static Error FailToReserveStockForOrder(Guid orderId, Guid launchId)
            => Error.Conflict(
                "Launches.FailToReserveStockForOrder",
                $"Fail to reserve stock for order {orderId} and launch {launchId}");
    }
}