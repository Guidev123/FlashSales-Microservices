using FlashSales.Domain.Results;
using FlashSales.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using Modules.Payments.Contracts;
using System.Net.Http.Json;
using static Modules.Payments.Contracts.IPaymentsPublicApi;

namespace Modules.Orders.Infrastructure.Services
{
    internal sealed class PaymentsApiService(HttpClient client, ILogger<PaymentsApiService> logger) : IPaymentsPublicApi
    {
        public Task<Result<InitiateCheckoutResponse>> InitiateCheckoutAsync(InitiateCheckoutRequest request, CancellationToken cancellationToken = default)
        {
            return client.PostAsJsonAsync
                ("api/v1/payments/checkout",
                request,
                cancellationToken
                ).ToResultAsync<InitiateCheckoutResponse>(logger, ct: cancellationToken);
        }
    }
}