using FlashSales.Domain.DomainObjects;
using FluentAssertions;
using Modules.Orders.Domain.Orders.Entities;
using Modules.Orders.Domain.Orders.Enums;

namespace Modules.Orders.IntegrationTests.Domain
{
    /// <summary>
    /// Pure domain tests for the event-sourced <see cref="Order"/> aggregate — no database, no Marten,
    /// only the public Create/behavior surface. Since Create/Confirm/Cancel/Refund/MarkPaymentProcessing
    /// all mutate exclusively through the same Apply(event) overloads Marten uses to replay a stream,
    /// asserting on the resulting state after a sequence of calls is equivalent to asserting that
    /// reconstruction from a persisted event history is correct.
    /// </summary>
    public sealed class OrderTests
    {
        private static readonly Guid CustomerId = Guid.NewGuid();
        private static readonly Guid LaunchId = Guid.NewGuid();
        private static readonly Guid SellerId = Guid.NewGuid();
        private static readonly Guid ProductId = Guid.NewGuid();

        private static Order CreateOrder(int quantity = 2, decimal unitPrice = 50m) =>
            Order.Create(CustomerId, LaunchId, SellerId, ProductId, quantity, unitPrice);

        [Fact]
        public void Create_WithValidInputs_ShouldInitializeStateFromTheCreatedEvent()
        {
            var order = CreateOrder(quantity: 3, unitPrice: 25m);

            order.CustomerId.Should().Be(CustomerId);
            order.LaunchId.Should().Be(LaunchId);
            order.SellerId.Should().Be(SellerId);
            order.ProductId.Should().Be(ProductId);
            order.Quantity.Should().Be(3);
            order.UnitPrice.Should().Be(25m);
            order.TotalAmount.Should().Be(75m);
            order.OrderCode.Should().StartWith("ORD-").And.HaveLength(12);
            order.Status.Should().Be(OrderStatus.AwaitingPayment);
            // CreatedOn is restored from the event's OccurredOn (set a few ticks after ExpiresAt was
            // computed from the pre-Apply constructor timestamp) — assert the window, not bit-exact equality.
            order.ExpiresAt.Should().BeCloseTo(order.CreatedOn.AddMinutes(Order.PaymentWindowMinutes), TimeSpan.FromSeconds(1));
            order.ConfirmedAt.Should().BeNull();
            order.Reason.Should().BeNull();

            order.DomainEvents.Should().ContainSingle();
        }

        [Theory]
        [InlineData(-1, 10)]
        [InlineData(-100, 10)]
        public void Create_WithNegativeQuantity_ShouldThrow(int quantity, decimal unitPrice)
        {
            var act = () => Order.Create(CustomerId, LaunchId, SellerId, ProductId, quantity, unitPrice);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void Create_WithNegativeUnitPrice_ShouldThrow()
        {
            var act = () => Order.Create(CustomerId, LaunchId, SellerId, ProductId, 1, -1m);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void MarkPaymentProcessing_FromAwaitingPayment_ShouldTransitionAndRaiseEvent()
        {
            var order = CreateOrder();
            order.ClearDomainEvents();

            var result = order.MarkPaymentProcessing();

            result.IsSuccess.Should().BeTrue();
            order.Status.Should().Be(OrderStatus.PaymentProcessing);
            order.DomainEvents.Should().ContainSingle();
        }

        [Fact]
        public void MarkPaymentProcessing_WhenAlreadyPaymentProcessing_ShouldBeIdempotentAndNotRaiseEvent()
        {
            var order = CreateOrder();
            order.MarkPaymentProcessing();
            order.ClearDomainEvents();

            var result = order.MarkPaymentProcessing();

            result.IsSuccess.Should().BeTrue();
            order.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public void MarkPaymentProcessing_WhenAlreadyConfirmed_ShouldFail()
        {
            var order = CreateOrder();
            order.MarkPaymentProcessing();
            order.Confirm();

            var result = order.MarkPaymentProcessing();

            result.IsFailure.Should().BeTrue();
        }

        [Fact]
        public void Confirm_FromPaymentProcessing_ShouldTransitionAndSetConfirmedAt()
        {
            var order = CreateOrder();
            order.MarkPaymentProcessing();
            order.ClearDomainEvents();

            var result = order.Confirm();

            result.IsSuccess.Should().BeTrue();
            order.Status.Should().Be(OrderStatus.Confirmed);
            order.ConfirmedAt.Should().NotBeNull();
            order.DomainEvents.Should().ContainSingle();
        }

        [Fact]
        public void Confirm_WhenCancelled_ShouldFail()
        {
            var order = CreateOrder();
            order.Cancel("out of stock");

            var result = order.Confirm();

            result.IsFailure.Should().BeTrue();
        }

        [Fact]
        public void Cancel_FromAwaitingPayment_ShouldTransitionAndSetReason()
        {
            var order = CreateOrder();
            order.ClearDomainEvents();

            var result = order.Cancel("customer requested");

            result.IsSuccess.Should().BeTrue();
            order.Status.Should().Be(OrderStatus.Cancelled);
            order.Reason.Should().Be("customer requested");
            order.DomainEvents.Should().ContainSingle();
        }

        [Fact]
        public void Cancel_WhenAlreadyCancelled_ShouldBeIdempotentAndNotRaiseEvent()
        {
            var order = CreateOrder();
            order.Cancel("first reason");
            order.ClearDomainEvents();

            var result = order.Cancel("second reason");

            result.IsSuccess.Should().BeTrue();
            order.Reason.Should().Be("first reason");
            order.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public void Cancel_WhenConfirmed_ShouldFail()
        {
            var order = CreateOrder();
            order.MarkPaymentProcessing();
            order.Confirm();

            var result = order.Cancel("too late");

            result.IsFailure.Should().BeTrue();
        }

        [Fact]
        public void MarkExpired_ShouldCancelWithExpiredReason()
        {
            var order = CreateOrder();

            var result = order.MarkExpired();

            result.IsSuccess.Should().BeTrue();
            order.Status.Should().Be(OrderStatus.Cancelled);
            order.Reason.Should().Be("Payment window expired");
        }

        [Fact]
        public void Refund_FromConfirmed_ShouldTransitionAndSetReason()
        {
            var order = CreateOrder();
            order.MarkPaymentProcessing();
            order.Confirm();
            order.ClearDomainEvents();

            var result = order.Refund("customer requested refund");

            result.IsSuccess.Should().BeTrue();
            order.Status.Should().Be(OrderStatus.Refunded);
            order.Reason.Should().Be("customer requested refund");
            order.DomainEvents.Should().ContainSingle();
        }

        [Fact]
        public void Refund_WhenAlreadyRefunded_ShouldBeIdempotentAndNotRaiseEvent()
        {
            var order = CreateOrder();
            order.Refund("first");
            order.ClearDomainEvents();

            var result = order.Refund("second");

            result.IsSuccess.Should().BeTrue();
            order.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public void FullLifecycle_CreateThenProcessThenConfirmThenRefund_ShouldReplayToTheExpectedFinalState()
        {
            var order = CreateOrder(quantity: 4, unitPrice: 10m);

            order.MarkPaymentProcessing();
            order.Confirm();
            order.Refund("customer changed their mind");

            order.Status.Should().Be(OrderStatus.Refunded);
            order.TotalAmount.Should().Be(40m);
            order.ConfirmedAt.Should().NotBeNull();
            order.Reason.Should().Be("customer changed their mind");
            order.DomainEvents.Should().HaveCount(4);
        }
    }
}
