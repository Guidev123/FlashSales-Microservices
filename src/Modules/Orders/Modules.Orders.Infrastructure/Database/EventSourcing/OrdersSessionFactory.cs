using Marten;

namespace Modules.Orders.Infrastructure.Database.EventSourcing
{
    internal sealed class OrdersSessionFactory(IDocumentStore store, OutboxDomainEventListener outboxDomainEventListener) : ISessionFactory
    {
        public IQuerySession QuerySession() => store.QuerySession();

        public IDocumentSession OpenSession()
        {
            var session = store.LightweightSession();

            session.Listeners.Add(outboxDomainEventListener);

            return session;
        }
    }
}