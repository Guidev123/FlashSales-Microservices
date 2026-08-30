using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Marten;
using Marten.Subscriptions;
using Modules.Orders.Domain.Orders.DomainEvents;
using MongoDB.Driver;

namespace Modules.Orders.Infrastructure.Database.EventSourcing
{
    internal sealed class MongoOrderProjectionSubscription(IMongoDatabase database) : SubscriptionBase
    {
        private readonly IMongoCollection<OrderReadModel> _collection = database.GetCollection<OrderReadModel>("orders");

        public override async Task<IChangeListener> ProcessEventsAsync(
            EventRange page,
            ISubscriptionController controller,
            IDocumentOperations operations,
            CancellationToken cancellationToken)
        {
            foreach (var e in page.Events)
            {
                switch (e.Data)
                {
                    case OrderCreatedDomainEvent created:
                        await _collection.ReplaceOneAsync(
                            x => x.Id == created.OrderId,
                            new OrderReadModel
                            {
                                Id = created.OrderId,
                                CustomerId = created.CustomerId,
                                LaunchId = created.LaunchId,
                                ProductId = created.ProductId,
                                SellerId = created.SellerId,
                                Quantity = created.Quantity,
                                UnitPrice = created.UnitPrice,
                                TotalAmount = created.Quantity * created.UnitPrice,
                                OrderCode = $"ORD-{created.OrderId.ToString("N")[..8].ToUpperInvariant()}",
                                Status = created.Status.ToString(),
                                ExpiresAt = created.ExpiresAt,
                                CreatedOn = e.Timestamp
                            },
                            new ReplaceOptions { IsUpsert = true },
                            cancellationToken);
                        break;

                    case OrderPaymentProcessingStartedDomainEvent processing:
                        await _collection.UpdateOneAsync(
                            x => x.Id == processing.OrderId,
                            Builders<OrderReadModel>.Update.Set(x => x.Status, processing.Status.ToString()),
                            cancellationToken: cancellationToken);
                        break;

                    case OrderConfirmedDomainEvent confirmed:
                        await _collection.UpdateOneAsync(
                            x => x.Id == confirmed.OrderId,
                            Builders<OrderReadModel>.Update
                                .Set(x => x.Status, confirmed.Status.ToString())
                                .Set(x => x.ConfirmedAt, confirmed.ConfirmedAt),
                            cancellationToken: cancellationToken);
                        break;

                    case OrderCancelledDomainEvent cancelled:
                        await _collection.UpdateOneAsync(
                            x => x.Id == cancelled.OrderId,
                            Builders<OrderReadModel>.Update
                                .Set(x => x.Status, cancelled.Status.ToString())
                                .Set(x => x.Reason, cancelled.Reason),
                            cancellationToken: cancellationToken);
                        break;

                    case OrderRefundedDomainEvent refunded:
                        await _collection.UpdateOneAsync(
                            x => x.Id == refunded.OrderId,
                            Builders<OrderReadModel>.Update
                                .Set(x => x.Status, refunded.Status.ToString())
                                .Set(x => x.Reason, refunded.Reason),
                            cancellationToken: cancellationToken);
                        break;
                }
            }

            return NullChangeListener.Instance;
        }
    }
}
