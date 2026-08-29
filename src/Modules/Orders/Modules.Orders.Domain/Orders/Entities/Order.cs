using FlashSales.Domain.DomainObjects;
using FlashSales.Domain.Results;
using Modules.Orders.Domain.Orders.DomainEvents;
using Modules.Orders.Domain.Orders.Enums;
using Modules.Orders.Domain.Orders.Errors;

namespace Modules.Orders.Domain.Orders.Entities
{
    public sealed class Order : Entity, IAggregateRoot
    {
        public const int PaymentWindowMinutes = 10;

        private Order()
        { }

        public Guid CustomerId { get; private set; }
        public Guid LaunchId { get; private set; }
        public Guid SellerId { get; private set; }
        public Guid ProductId { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal TotalAmount { get; private set; }
        public string OrderCode { get; private set; } = null!;
        public OrderStatus Status { get; private set; }
        public DateTimeOffset ExpiresAt { get; private set; }
        public DateTimeOffset? ConfirmedAt { get; private set; }
        public string? Reason { get; private set; }

        public static Order Create(
            Guid customerId,
            Guid launchId,
            Guid sellerId,
            Guid productId,
            int quantity,
            decimal unitPrice
            )
        {
            var order = new Order();

            var expiresAt = order.CreatedOn.AddMinutes(PaymentWindowMinutes);

            var orderCreatedDomainEvent = OrderCreatedDomainEvent.Create(
                order.Id,
                customerId,
                sellerId,
                launchId,
                productId,
                unitPrice,
                quantity,
                expiresAt);

            order.Apply(orderCreatedDomainEvent);
            order.Validate();

            order.AddDomainEvent(orderCreatedDomainEvent);

            return order;
        }

        public Result MarkPaymentProcessing()
        {
            if (Status == OrderStatus.PaymentProcessing) return Result.Success();

            if (Status != OrderStatus.AwaitingPayment)
                return Result.Failure(OrderErrors.InvalidStatusTransition(Status.ToString(), OrderStatus.PaymentProcessing.ToString()));

            var paymentInProcessingDomainEvent = OrderPaymentProcessingStartedDomainEvent.Create(Id);

            Apply(paymentInProcessingDomainEvent);
            AddDomainEvent(paymentInProcessingDomainEvent);

            return Result.Success();
        }

        public Result Confirm()
        {
            if (Status == OrderStatus.Confirmed) return Result.Success();

            if (Status == OrderStatus.Cancelled || Status == OrderStatus.Refunded)
                return Result.Failure(OrderErrors.InvalidStatusTransition(Status.ToString(), OrderStatus.Confirmed.ToString()));

            var orderConfirmedDomainEvent = OrderConfirmedDomainEvent.Create(Id, CustomerId, LaunchId, Quantity);

            Apply(orderConfirmedDomainEvent);
            AddDomainEvent(orderConfirmedDomainEvent);

            return Result.Success();
        }

        public Result Cancel(string reason)
        {
            if (Status == OrderStatus.Cancelled) return Result.Success();

            if (Status == OrderStatus.Confirmed || Status == OrderStatus.Refunded)
                return Result.Failure(OrderErrors.InvalidStatusTransition(Status.ToString(), OrderStatus.Cancelled.ToString()));

            var orderCancelledDomainEvent = OrderCancelledDomainEvent.Create(Id, CustomerId, LaunchId, Quantity, reason);

            Apply(orderCancelledDomainEvent);
            AddDomainEvent(orderCancelledDomainEvent);

            return Result.Success();
        }

        public Result MarkExpired() => Cancel("Payment window expired");

        public Result Refund(string reason)
        {
            if (Status == OrderStatus.Refunded) return Result.Success();

            var orderRefundedDomainEvent = OrderRefundedDomainEvent.Create(Id, CustomerId, LaunchId, Quantity, reason);

            Apply(orderRefundedDomainEvent);
            AddDomainEvent(orderRefundedDomainEvent);

            return Result.Success();
        }

        public void Apply(OrderCreatedDomainEvent domainEvent)
        {
            Id = domainEvent.OrderId;
            CustomerId = domainEvent.CustomerId;
            SellerId = domainEvent.SellerId;
            LaunchId = domainEvent.LaunchId;
            Quantity = domainEvent.Quantity;
            ProductId = domainEvent.ProductId;
            UnitPrice = domainEvent.UnitPrice;
            TotalAmount = domainEvent.Quantity * domainEvent.UnitPrice;
            OrderCode = $"ORD-{Id.ToString("N")[..8].ToUpperInvariant()}";
            Status = domainEvent.Status;
            ExpiresAt = domainEvent.ExpiresAt;
        }

        public void Apply(OrderPaymentProcessingStartedDomainEvent domainEvent)
        {
            Status = domainEvent.Status;
        }

        public void Apply(OrderConfirmedDomainEvent domainEvent)
        {
            Status = domainEvent.Status;
            ConfirmedAt = domainEvent.ConfirmedAt;
        }

        public void Apply(OrderCancelledDomainEvent domainEvent)
        {
            Status = domainEvent.Status;
            Reason = domainEvent.Reason;
        }

        public void Apply(OrderRefundedDomainEvent domainEvent)
        {
            Status = domainEvent.Status;
            Reason = domainEvent.Reason;
        }

        protected override void Validate()
        {
            AssertionConcern.EnsureTrue(CustomerId != Guid.Empty, OrderErrors.CustomerIdRequired.Description);
            AssertionConcern.EnsureTrue(LaunchId != Guid.Empty, OrderErrors.LaunchIdRequired.Description);
            AssertionConcern.EnsureGreaterThan(Quantity, 0, OrderErrors.QuantityMustBeAtLeastOne.Description);
            AssertionConcern.EnsureGreaterThan(UnitPrice, 0, OrderErrors.UnitPriceMustBeGreaterThanZero.Description);
        }
    }
}