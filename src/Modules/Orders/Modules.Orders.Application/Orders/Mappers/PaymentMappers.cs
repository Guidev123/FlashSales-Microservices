using Modules.Payments.Contracts;
using static Modules.Payments.Contracts.IPaymentsPublicApi;

namespace Modules.Orders.Application.Orders.Mappers
{
    internal static class PaymentMappers
    {
        public static InitiateCheckoutRequest MapToRequest(
            Guid orderId,
            string orderCode,
            decimal totalAmount,
            Guid customerId,
            string customerEmail,
            IReadOnlyCollection<CheckoutLineItem> items
            )
        {
            return new InitiateCheckoutRequest(
                orderId,
                orderCode,
                totalAmount,
                items
                );
        }
    }
}