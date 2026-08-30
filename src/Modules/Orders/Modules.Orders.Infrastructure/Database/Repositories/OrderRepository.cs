using JasperFx;
using JasperFx.Events;
using Marten;
using Marten.Services;
using Modules.Orders.Domain.Orders.Entities;
using Modules.Orders.Domain.Orders.Enums;
using Modules.Orders.Domain.Orders.Repositories;
using Modules.Orders.Infrastructure.Database.EventSourcing;
using Npgsql;
using System.Data;
using UnitOfWorkInterface = FlashSales.Application.Abstractions.IUnitOfWork;

namespace Modules.Orders.Infrastructure.Database.Repositories
{
    internal sealed class OrderRepository(
        IDocumentStore store,
        UnitOfWorkInterface unitOfWork,
        OutboxDomainEventListener outboxDomainEventListener) : IOrderRepository, IDisposable, IAsyncDisposable
    {
        private IDocumentSession? _session;
        private IDbTransaction? _sessionBoundTransaction;
        private IEventStream<Order>? _stream;

        public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _stream = await Session.Events.FetchForWriting<Order>(id, cancellationToken);

            return _stream.Aggregate;
        }

        public async Task StartStreamAsync(Order order, CancellationToken cancellationToken = default)
        {
            Session.Events.StartStream<Order>(order.Id, order.DomainEvents);

            try
            {
                await Session.SaveChangesAsync(cancellationToken);
            }
            catch (DocumentAlreadyExistsException)
            {
                throw new ActiveOrderAlreadyExistsException(order.LaunchId);
            }
        }

        public async Task AppendAsync(Order order, CancellationToken cancellationToken = default)
        {
            if (_stream is null || _stream.Id != order.Id)
                _stream = await Session.Events.FetchForWriting<Order>(order.Id, cancellationToken);

            _stream.AppendMany(order.DomainEvents);

            await Session.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<Guid>> GetStaleAwaitingOrProcessingAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTimeOffset.UtcNow;

            return await Session.Query<Order>()
                .Where(o =>
                    (o.Status == OrderStatus.AwaitingPayment || o.Status == OrderStatus.PaymentProcessing) &&
                    o.ExpiresAt < now)
                .Select(o => o.Id)
                .ToListAsync(cancellationToken);
        }

        // A single repository instance (one per DI scope) can outlive more than one EF transaction —
        // e.g. two separate mediator.SendAsync calls sharing the same test/scope each go through their
        // own RequestTransactionBehavior begin/commit cycle. Reusing a session bound to an already
        // completed transaction throws ("Transaction is already completed"), so rebuild whenever the
        // ambient transaction has changed (including transitioning to/from no transaction at all).
        private IDocumentSession Session
        {
            get
            {
                if (_session is null || !ReferenceEquals(_sessionBoundTransaction, unitOfWork.Transaction))
                {
                    _session?.Dispose();
                    _stream = null;
                    _sessionBoundTransaction = unitOfWork.Transaction;
                    _session = CreateSession();
                }

                return _session;
            }
        }

        private IDocumentSession CreateSession()
        {
            var session = unitOfWork.Transaction is not null and NpgsqlTransaction transaction
                ? store.LightweightSession(SessionOptions.ForTransaction(transaction))
                : store.LightweightSession();

            session.Listeners.Add(outboxDomainEventListener);

            return session;
        }

        public void Dispose()
        {
            _session?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (_session is not null) await _session.DisposeAsync();
        }
    }
}