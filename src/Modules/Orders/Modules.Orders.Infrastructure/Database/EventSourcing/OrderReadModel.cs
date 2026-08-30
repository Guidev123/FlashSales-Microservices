using MongoDB.Bson.Serialization.Attributes;

namespace Modules.Orders.Infrastructure.Database.EventSourcing
{
    public sealed class OrderReadModel
    {
        [BsonId]
        public Guid Id { get; set; }

        public Guid CustomerId { get; set; }

        public Guid LaunchId { get; set; }

        public Guid ProductId { get; set; }

        public Guid SellerId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalAmount { get; set; }

        public string OrderCode { get; set; } = null!;

        public string Status { get; set; } = null!;

        public DateTimeOffset ExpiresAt { get; set; }

        public DateTimeOffset? ConfirmedAt { get; set; }

        public string? Reason { get; set; }

        public DateTimeOffset CreatedOn { get; set; }
    }
}
