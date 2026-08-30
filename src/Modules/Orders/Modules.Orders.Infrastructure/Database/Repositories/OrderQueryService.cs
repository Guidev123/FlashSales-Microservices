using Dapper;
using FlashSales.Application.Abstractions;
using Modules.Orders.Application.Orders.Dtos;
using Modules.Orders.Application.Orders.Services;
using Modules.Orders.Infrastructure.Database.EventSourcing;
using MongoDB.Driver;

namespace Modules.Orders.Infrastructure.Database.Repositories
{
    internal sealed class OrderQueryService(IUnitOfWork unitOfWork, IMongoDatabase mongoDatabase) : IOrderQueryService
    {
        private readonly IMongoCollection<OrderReadModel> _collection = mongoDatabase.GetCollection<OrderReadModel>("orders");

        public async Task<OrderResponse?> GetByIdAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken = default)
        {
            var order = await _collection
                .Find(o => o.Id == orderId && o.CustomerId == customerId)
                .FirstOrDefaultAsync(cancellationToken);

            if (order is null) return null;

            var launchTitle = await GetLaunchTitleAsync(order.LaunchId);

            return MapToResponse(order, launchTitle);
        }

        public async Task<IReadOnlyCollection<OrderResponse>> GetByCustomerAsync(
            Guid customerId,
            int page,
            int size,
            CancellationToken cancellationToken = default)
        {
            var orders = await _collection
                .Find(o => o.CustomerId == customerId)
                .SortByDescending(o => o.CreatedOn)
                .Skip((page - 1) * size)
                .Limit(size)
                .ToListAsync(cancellationToken);

            if (orders.Count == 0) return [];

            var launchTitles = await GetLaunchTitlesAsync(orders.Select(o => o.LaunchId).Distinct());

            return orders
                .Select(o => MapToResponse(o, launchTitles.GetValueOrDefault(o.LaunchId)))
                .ToList()
                .AsReadOnly();
        }

        public Task<int> GetByCustomerTotalCountAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return _collection
                .CountDocumentsAsync(o => o.CustomerId == customerId, cancellationToken: cancellationToken)
                .ContinueWith(t => (int)t.Result, cancellationToken);
        }

        public async Task<bool> HasActiveOrderAsync(Guid customerId, Guid launchId, CancellationToken cancellationToken = default)
        {
            var count = await _collection.CountDocumentsAsync(
                o => o.CustomerId == customerId
                     && o.LaunchId == launchId
                     && (o.Status == "AwaitingPayment" || o.Status == "PaymentProcessing"),
                cancellationToken: cancellationToken);

            return count > 0;
        }

        public async Task<int> GetConfirmedQuantityAsync(Guid customerId, Guid launchId, CancellationToken cancellationToken = default)
        {
            var confirmedOrders = await _collection
                .Find(o => o.CustomerId == customerId && o.LaunchId == launchId && o.Status == "Confirmed")
                .Project(o => o.Quantity)
                .ToListAsync(cancellationToken);

            return confirmedOrders.Sum();
        }

        private async Task<string?> GetLaunchTitleAsync(Guid launchId)
        {
            const string sql = """SELECT "Title" FROM orders."Launches" WHERE "Id" = @LaunchId""";

            return await unitOfWork.Connection.QuerySingleOrDefaultAsync<string?>(sql, new { LaunchId = launchId });
        }

        private async Task<Dictionary<Guid, string?>> GetLaunchTitlesAsync(IEnumerable<Guid> launchIds)
        {
            const string sql = """SELECT "Id", "Title" FROM orders."Launches" WHERE "Id" = ANY(@LaunchIds)""";

            var rows = await unitOfWork.Connection.QueryAsync(sql, new { LaunchIds = launchIds.ToArray() });

            return rows.ToDictionary(r => (Guid)r.Id, r => (string?)r.Title);
        }

        private static OrderResponse MapToResponse(OrderReadModel order, string? launchTitle)
        {
            return new OrderResponse(
                Id: order.Id,
                LaunchId: order.LaunchId,
                LaunchTitle: launchTitle,
                ProductId: order.ProductId,
                SellerId: order.SellerId,
                Quantity: order.Quantity,
                UnitPrice: order.UnitPrice,
                TotalAmount: order.TotalAmount,
                OrderCode: order.OrderCode,
                Status: order.Status,
                ExpiresAt: order.ExpiresAt,
                ConfirmedAt: order.ConfirmedAt,
                Reason: order.Reason,
                CreatedOn: order.CreatedOn
                );
        }
    }
}
