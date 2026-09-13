using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Coupons.Application.Coupons.Features.Create;
using Modules.Coupons.Domain.Coupons.Enums;
using Modules.Coupons.IntegrationTests.Abstractions;
using Modules.Coupons.IntegrationTests.Abstractions.Helpers;

namespace Modules.Coupons.IntegrationTests.Features.Coupons
{
    public sealed class CreateCouponTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task CreateCoupon_WhenCodeDoesNotExist_ShouldCreateDraftCoupon()
        {
            // Arrange
            var code = $"CODE-{_faker.Random.AlphaNumeric(8).ToUpperInvariant()}";

            // Act
            var result = await CouponHelper.CreateAsync(_factory, _faker, code: code, discountPercentage: 15);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var coupon = await _dbContext.Coupons.FirstAsync(c => c.Code == code);
            coupon.Status.Should().Be(CouponStatus.Draft);
            coupon.Discount.Type.Should().Be(CouponType.Percentage);
            coupon.Discount.Value.Should().Be(15);
            coupon.Usage.MaxRedemptions.Should().Be(100);
            coupon.Usage.RedeemedCount.Should().Be(0);
        }

        [Fact]
        public async Task CreateCoupon_WithFixedDiscount_ShouldPersistFixedDiscount()
        {
            // Act
            var result = await CouponHelper.CreateAsync(
                _factory, _faker, discountAmount: 25m, discountPercentage: null);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var coupon = await _dbContext.Coupons.OrderByDescending(c => c.CreatedOn).FirstAsync();
            coupon.Discount.Type.Should().Be(CouponType.Fixed);
            coupon.Discount.Value.Should().Be(25m);
        }

        [Fact]
        public async Task CreateCoupon_WhenCodeAlreadyExists_ShouldNotCreateDuplicate()
        {
            // Arrange
            var code = $"CODE-{_faker.Random.AlphaNumeric(8).ToUpperInvariant()}";
            await CouponHelper.CreateAsync(_factory, _faker, code: code);

            // Act
            var result = await CouponHelper.CreateAsync(_factory, _faker, code: code);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var count = await _dbContext.Coupons.CountAsync(c => c.Code == code);
            count.Should().Be(1);
        }

        [Fact]
        public async Task CreateCoupon_WhenValidFromIsAfterValidUntil_ShouldReturnFailure()
        {
            // Act
            var result = await CouponHelper.CreateAsync(
                _factory,
                _faker,
                validFrom: DateTimeOffset.UtcNow.AddDays(10),
                validUntil: DateTimeOffset.UtcNow);

            // Assert
            result.IsFailure.Should().BeTrue();
        }

        [Fact]
        public async Task CreateCoupon_WhenMaxRedemptionsIsZero_ShouldReturnFailure()
        {
            // Act
            var result = await CouponHelper.CreateAsync(_factory, _faker, maxRedemptions: 0);

            // Assert
            result.IsFailure.Should().BeTrue();
        }

        [Fact]
        public async Task CreateCoupon_WhenDiscountPercentageIsOutOfRange_ShouldReturnFailure()
        {
            // Act
            var result = await CouponHelper.CreateAsync(
                _factory, _faker, discountAmount: null, discountPercentage: 150);

            // Assert
            result.IsFailure.Should().BeTrue();
        }
    }
}
