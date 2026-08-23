using FlashSales.Domain.Results;

namespace Modules.Launches.Contracts
{
    public interface ILaunchesPublicApi
    {
        Task<Result> ReserveAsync(
            ReserveLaunchRequest request,
            CancellationToken cancellationToken = default
            );

        public sealed record ReserveLaunchRequest(
            Guid LaunchId,
            int Quantity,
            Guid OrderId
            );

        Task<Result> ReleaseAsync(
            ReleaseLaunchRequest request,
            CancellationToken cancellationToken = default
            );

        public sealed record ReleaseLaunchRequest(
            Guid LaunchId,
            int Quantity,
            Guid OrderId
            );
    }
}