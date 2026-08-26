using FlashSales.Domain.Results;
using FlashSales.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using Modules.Launches.Contracts;
using System.Net.Http.Json;
using static Modules.Launches.Contracts.ILaunchesPublicApi;

namespace Modules.Orders.Infrastructure.Services
{
    internal sealed class LaunchesApiService(HttpClient client, ILogger<LaunchesApiService> logger) : ILaunchesPublicApi
    {
        public Task<Result> ReleaseAsync(ReleaseLaunchRequest request, CancellationToken cancellationToken = default)
        {
            return client.PostAsJsonAsync(
                "api/v1/launches/stock/release",
                request,
                cancellationToken
                ).ToResultAsync(logger, ct: cancellationToken);
        }

        public Task<Result> ReserveAsync(ReserveLaunchRequest request, CancellationToken cancellationToken = default)
        {
            return client.PostAsJsonAsync(
                "api/v1/launches/stock/reserve",
                request,
                cancellationToken
                ).ToResultAsync(logger, ct: cancellationToken);
        }
    }
}