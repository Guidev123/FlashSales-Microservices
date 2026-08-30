using FluentAssertions;
using Modules.Orders.Application.Orders.Features.GetById;
using Modules.Orders.Domain.Orders.Errors;
using Modules.Orders.IntegrationTests.Abstractions;
using Modules.Orders.IntegrationTests.Abstractions.Helpers;

namespace Modules.Orders.IntegrationTests.Features.Orders
{
    public sealed class GetOrderByIdTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task GetOrderById_WhenOrderBelongsToDifferentCustomer_ShouldReturnNotFound()
        {
            // Arrange
            var (_, _, orderId, _) = await OrderHelper.CreateAwaitingPaymentAsync(_factory, _faker);

            // Act
            var result = await _mediator.SendAsync(new GetOrderByIdQuery(orderId, Guid.NewGuid()));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(OrderErrors.NotFound(orderId).Code);
        }

        [Fact]
        public async Task GetOrderById_WhenOrderDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var orderId = Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new GetOrderByIdQuery(orderId, Guid.NewGuid()));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(OrderErrors.NotFound(orderId).Code);
        }
    }
}
