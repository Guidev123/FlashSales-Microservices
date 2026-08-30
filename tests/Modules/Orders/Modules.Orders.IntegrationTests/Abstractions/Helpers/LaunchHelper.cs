using Bogus;
using Microsoft.Extensions.DependencyInjection;
using MidR.Interfaces;
using Modules.Orders.Domain.Launches.Enums;
using ReplicaCreateLaunchCommand = Modules.Orders.Application.Launches.Features.Create.CreateLaunchCommand;

namespace Modules.Orders.IntegrationTests.Abstractions.Helpers
{
    internal sealed record RealLaunch(
        Guid UserId,
        Guid SellerId,
        Guid ProductId,
        Guid LaunchId,
        string Title,
        decimal DiscountedPrice,
        decimal OriginalPrice,
        int TotalQuantity,
        DateTimeOffset StartAt,
        DateTimeOffset EndAt);

    internal static class LaunchHelper
    {
        internal const decimal DiscountedPrice = 50m;
        internal const decimal OriginalPrice = 100m;

        internal static async Task<RealLaunch> CreateActiveAsync(
            IntegrationWebApplicationFactory factory,
            Faker faker,
            int totalQuantity = 10,
            LaunchSaleType saleType = LaunchSaleType.Quantity)
        {
            var seller = SellerHelper.Create(faker);
            var productId = Guid.NewGuid();
            var launchId = Guid.NewGuid();
            var title = faker.Commerce.ProductName();
            var startAt = DateTimeOffset.UtcNow.AddMinutes(5);
            var endAt = DateTimeOffset.UtcNow.AddHours(2);

            await ReplicateAsync(
                factory,
                launchId,
                seller.SellerId,
                productId,
                title,
                DiscountedPrice,
                OriginalPrice,
                totalQuantity,
                startAt,
                endAt,
                saleType);

            return new RealLaunch(
                seller.UserId,
                seller.SellerId,
                productId,
                launchId,
                title,
                DiscountedPrice,
                OriginalPrice,
                totalQuantity,
                startAt,
                endAt);
        }

        internal static async Task ReplicateAsync(
            IntegrationWebApplicationFactory factory,
            Guid launchId,
            Guid sellerId,
            Guid productId,
            string title,
            decimal discountedPrice,
            decimal originalPrice,
            int totalQuantity,
            DateTimeOffset startAt,
            DateTimeOffset endAt,
            LaunchSaleType saleType = LaunchSaleType.Quantity)
        {
            await using var scope = factory.Services.CreateAsyncScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            await mediator.SendAsync(new ReplicaCreateLaunchCommand(
                launchId,
                sellerId,
                productId,
                title,
                discountedPrice,
                originalPrice,
                totalQuantity,
                startAt,
                endAt,
                saleType.ToString()));
        }
    }
}
