using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Orders.Domain.Launches.Entities;
using Modules.Orders.Domain.Launches.Enums;
using Modules.Orders.Domain.Launches.Repositories;

namespace Modules.Orders.Application.Launches.Features.Create
{
    internal sealed class CreateLaunchCommandHandler(
        ILaunchRepository launchRepository
        ) : ICommandHandler<CreateLaunchCommand>
    {
        public async Task<Result> ExecuteAsync(CreateLaunchCommand request, CancellationToken cancellationToken = default)
        {
            var alreadyExists = await launchRepository.ExistsAsync(request.LaunchId, cancellationToken);
            if (alreadyExists)
            {
                return Result.Success();
            }

            var saleType = Enum.Parse<LaunchSaleType>(request.SaleType, ignoreCase: true);

            var launch = Launch.Create(
                request.LaunchId,
                request.SellerId,
                request.ProductId,
                request.Title,
                request.DiscountedPrice,
                request.OriginalPrice,
                request.TotalQuantity,
                request.StartAt,
                request.EndAt,
                saleType
                );

            launchRepository.Add(launch);

            return Result.Success();
        }
    }
}
