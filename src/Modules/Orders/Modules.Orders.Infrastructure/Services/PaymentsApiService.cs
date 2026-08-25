using FlashSales.Domain.Results;
using Microsoft.Extensions.Logging;
using Modules.Payments.Contracts;
using static Modules.Payments.Contracts.IPaymentsPublicApi;

namespace Modules.Orders.Infrastructure.Services
{
    internal sealed class PaymentsApiService(HttpClient client, ILogger<PaymentsApiService> logger) : IPaymentsPublicApi
    {
        public Task<Result<InitiateCheckoutResponse>> InitiateCheckoutAsync(InitiateCheckoutRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
