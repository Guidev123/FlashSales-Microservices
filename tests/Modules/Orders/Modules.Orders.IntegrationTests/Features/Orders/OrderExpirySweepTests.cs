using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Orders.Application.Orders.Features.Confirm;
using Modules.Orders.Application.Orders.Features.Expire;
using Modules.Orders.Domain.Orders.Enums;
using Modules.Orders.Domain.Orders.Repositories;
using Modules.Orders.IntegrationTests.Abstractions;
using Modules.Orders.IntegrationTests.Abstractions.Helpers;
using Npgsql;
using System.Text.Json;

namespace Modules.Orders.IntegrationTests.Features.Orders
{
    public sealed class OrderExpirySweepTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task GetStaleAwaitingOrProcessingAsync_WhenOrderExpiresAtIsInPast_ShouldReturnOrderId()
        {
            // Arrange
            var (_, _, orderId, _) = await OrderHelper.CreateAwaitingPaymentAsync(_factory, _faker);
            await ForceExpiredAsync(orderId);

            await using var scope = _factory.Services.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

            // Act
            var staleIds = await repository.GetStaleAwaitingOrProcessingAsync();

            // Assert
            staleIds.Should().Contain(orderId);
        }

        [Fact]
        public async Task GetStaleAwaitingOrProcessingAsync_WhenOrderIsNotExpiredYet_ShouldNotReturnIt()
        {
            // Arrange
            var (_, _, orderId, _) = await OrderHelper.CreateAwaitingPaymentAsync(_factory, _faker);

            await using var scope = _factory.Services.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

            // Act
            var staleIds = await repository.GetStaleAwaitingOrProcessingAsync();

            // Assert
            staleIds.Should().NotContain(orderId);
        }

        [Fact]
        public async Task GetStaleAwaitingOrProcessingAsync_WhenOrderIsAlreadyConfirmed_ShouldNotReturnIt()
        {
            // Arrange
            var (_, _, orderId, _) = await OrderHelper.CreateAwaitingPaymentAsync(_factory, _faker);
            await _mediator.SendAsync(new ConfirmOrderCommand(orderId));
            await ForceExpiredAsync(orderId);

            await using var scope = _factory.Services.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

            // Act
            var staleIds = await repository.GetStaleAwaitingOrProcessingAsync();

            // Assert
            staleIds.Should().NotContain(orderId);
        }

        [Fact]
        public async Task ExpireOrderCommand_SimulatingSweepJobBehavior_ShouldExpireStaleOrder()
        {
            // Arrange
            var (_, _, orderId, _) = await OrderHelper.CreateAwaitingPaymentAsync(_factory, _faker, launchTotalQuantity: 5, quantity: 1);
            await ForceExpiredAsync(orderId);

            await using var scope = _factory.Services.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var staleIds = await repository.GetStaleAwaitingOrProcessingAsync();
            staleIds.Should().Contain(orderId);

            // Act
            var result = await _mediator.SendAsync(new ExpireOrderCommand(orderId));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var order = (await GetOrderAsync(orderId))!;
            order.Status.Should().Be(OrderStatus.Cancelled);
        }

        // Order now lives as a Marten Inline document (orders_events.mt_doc_order), not the old EF table —
        // rewrite the "ExpiresAt" key directly inside the stored JSONB. The replacement value is produced
        // by the exact same serializer Marten uses (UseSystemTextJsonForSerialization), so it round-trips
        // correctly instead of risking a Postgres-vs-.NET date-format mismatch.
        private async Task ForceExpiredAsync(Guid orderId)
        {
            var expiredAtJson = JsonSerializer.Serialize(DateTimeOffset.UtcNow.AddHours(-1));

            await using var connection = new NpgsqlConnection(_factory.GetConnectionString());
            await connection.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                """UPDATE orders_events.mt_doc_order SET data = jsonb_set(data, '{ExpiresAt}', @ExpiresAt::jsonb) WHERE id = @OrderId""",
                connection);
            cmd.Parameters.AddWithValue("ExpiresAt", expiredAtJson);
            cmd.Parameters.AddWithValue("OrderId", orderId);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
