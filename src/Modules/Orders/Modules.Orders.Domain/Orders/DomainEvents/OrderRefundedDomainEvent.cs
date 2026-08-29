using FlashSales.Domain.DomainObjects;
using Modules.Orders.Domain.Orders.Enums;

namespace Modules.Orders.Domain.Orders.DomainEvents
{
    public sealed record OrderRefundedDomainEvent : DomainEvent
    {
        public static OrderRefundedDomainEvent Create(Guid orderId, Guid customerId, Guid launchId, int quantity, string reason)
        {
            return new OrderRefundedDomainEvent(orderId, customerId, launchId, quantity, reason);
        }

        private OrderRefundedDomainEvent(Guid orderId, Guid customerId, Guid launchId, int quantity, string reason)
            : base(orderId, nameof(OrderRefundedDomainEvent))
        {
            OrderId = orderId;
            CustomerId = customerId;
            LaunchId = launchId;
            Quantity = quantity;
            Reason = reason;
            Status = OrderStatus.Refunded;
        }

        private OrderRefundedDomainEvent()
        { }

        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid LaunchId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = null!;
        public OrderStatus Status { get; set; }
    }
}