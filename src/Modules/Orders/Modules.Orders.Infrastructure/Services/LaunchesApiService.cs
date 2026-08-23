using FlashSales.Domain.Results;
using Modules.Launches.Contracts;
using Modules.Orders.Domain.Launches.Errors;
using System.Net.Http.Json;
using static Modules.Launches.Contracts.ILaunchesPublicApi;

namespace Modules.Orders.Infrastructure.Services
{
    internal sealed class LaunchesApiService(HttpClient client) : ILaunchesPublicApi
    {
        public async Task<Result> ReleaseAsync(ReleaseLaunchRequest request, CancellationToken cancellationToken = default)
        {
            var response = await client.PostAsJsonAsync(
                $"api/v1/launches/{request.LaunchId}/stock/release",
                request,
                cancellationToken
                );

            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure(LaunchErrors.FailToReleaseStockForOrder(request.OrderId, request.LaunchId));
            }

            return Result.Success();
        }

        public async Task<Result> ReserveAsync(ReserveLaunchRequest request, CancellationToken cancellationToken = default)
        {
            var response = await client.PostAsJsonAsync(
                $"api/v1/launches/{request.LaunchId}/stock/reserve",
                request,
                cancellationToken
                );

            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure(LaunchErrors.FailToReserveStockForOrder(request.OrderId, request.LaunchId));
            }

            return Result.Success();
        }
    }
}