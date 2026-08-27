using FlashSales.Domain.Results;

namespace Modules.Payments.Contracts
{
    public interface IPaymentsPublicApi
    {
        Task<Result<InitiateCheckoutResponse>> InitiateCheckoutAsync(
            InitiateCheckoutRequest request,
            CancellationToken cancellationToken = default
            );

        public sealed record InitiateCheckoutRequest(
            Guid OrderId,
            string OrderCode,
            decimal Amount,
            IReadOnlyCollection<CheckoutLineItem> Items
            );
        public sealed record InitiateCheckoutResponse(
            Guid PaymentId,
            Guid AttemptId,
            string CheckoutUrl
            );
    }
}
