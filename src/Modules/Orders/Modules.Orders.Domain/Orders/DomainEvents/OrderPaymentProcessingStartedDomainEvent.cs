using FlashSales.Domain.DomainObjects;
using Modules.Orders.Domain.Orders.Enums;

namespace Modules.Orders.Domain.Orders.DomainEvents
{
    public sealed record OrderPaymentProcessingStartedDomainEvent : DomainEvent
    {
        public static OrderPaymentProcessingStartedDomainEvent Create(
            Guid orderId
            )
        {
            return new OrderPaymentProcessingStartedDomainEvent(orderId, OrderStatus.PaymentProcessing);
        }
        private OrderPaymentProcessingStartedDomainEvent(Guid orderId, OrderStatus status)
            : base(orderId, nameof(OrderPaymentProcessingStartedDomainEvent))
        {
            OrderId = orderId;
            Status = status;
        }

        private OrderPaymentProcessingStartedDomainEvent()
        { }

        public Guid OrderId { get; set; }
        public OrderStatus Status { get; set; }
    }
}