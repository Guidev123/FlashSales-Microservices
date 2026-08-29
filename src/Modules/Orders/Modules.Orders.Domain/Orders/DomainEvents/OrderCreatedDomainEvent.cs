using FlashSales.Domain.DomainObjects;
using Modules.Orders.Domain.Orders.Enums;

namespace Modules.Orders.Domain.Orders.DomainEvents
{
    public sealed record OrderCreatedDomainEvent : DomainEvent
    {
        public static OrderCreatedDomainEvent Create(
            Guid orderId,
            Guid customerId,
            Guid sellerId,
            Guid launchId,
            Guid productId,
            decimal unitPrice,
            int quantity,
            DateTimeOffset expiresAt
            )
        {
            return new OrderCreatedDomainEvent(orderId, customerId, sellerId, launchId, productId, unitPrice, quantity, expiresAt);
        }

        private OrderCreatedDomainEvent(
            Guid orderId,
            Guid customerId,
            Guid sellerId,
            Guid launchId,
            Guid productId,
            decimal unitPrice,
            int quantity,
            DateTimeOffset expiresAt
            )
            : base(orderId, nameof(OrderCreatedDomainEvent))
        {
            OrderId = orderId;
            CustomerId = customerId;
            SellerId = sellerId;
            LaunchId = launchId;
            Quantity = quantity;
            ProductId = productId;
            UnitPrice = unitPrice;
            ExpiresAt = expiresAt;
            Status = OrderStatus.AwaitingPayment;
        }

        private OrderCreatedDomainEvent()
        { }

        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid SellerId { get; set; }
        public Guid LaunchId { get; set; }
        public Guid ProductId { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public OrderStatus Status { get; set; }
    }
}