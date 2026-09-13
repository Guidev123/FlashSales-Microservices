using Bogus;
using FlashSales.Domain.Results;
using Microsoft.Extensions.DependencyInjection;
using MidR.Interfaces;
using Modules.Coupons.Application.Coupons.Features.Create;

namespace Modules.Coupons.IntegrationTests.Abstractions.Helpers
{
    internal static class CouponHelper
    {
        internal static async Task<Result> CreateAsync(
            IntegrationWebApplicationFactory factory,
            Faker faker,
            Guid? launchId = null,
            Guid? sellerId = null,
            string? code = null,
            int maxRedemptions = 100,
            DateTimeOffset? validFrom = null,
            DateTimeOffset? validUntil = null,
            decimal? minimumOrderAmount = null,
            int? maxRedemptionsPerCustomer = null,
            decimal? discountAmount = null,
            int? discountPercentage = 10,
            decimal? maxDiscountAmount = null)
        {
            await using var scope = factory.Services.CreateAsyncScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            return await mediator.SendAsync(new CreateCouponCommand(
                launchId ?? Guid.NewGuid(),
                sellerId ?? Guid.NewGuid(),
                code ?? $"CODE-{faker.Random.AlphaNumeric(8).ToUpperInvariant()}",
                maxRedemptions,
                validFrom ?? DateTimeOffset.UtcNow,
                validUntil ?? DateTimeOffset.UtcNow.AddDays(30),
                minimumOrderAmount,
                maxRedemptionsPerCustomer,
                discountAmount,
                discountAmount is null ? discountPercentage : null,
                maxDiscountAmount));
        }
    }
}
