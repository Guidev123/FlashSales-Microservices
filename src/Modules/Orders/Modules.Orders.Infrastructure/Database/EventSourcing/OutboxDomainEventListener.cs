using FlashSales.Application.Messaging;
using FlashSales.Domain.DomainObjects;
using Marten;

namespace Modules.Orders.Infrastructure.Database.EventSourcing
{
    internal sealed class OutboxDomainEventListener(IDomainEventCollector domainEventCollector) : DocumentSessionListenerBase
    {
        public override Task BeforeSaveChangesAsync(IDocumentSession session, CancellationToken token)
        {
            var domainEvents = session.PendingChanges.Streams()
                .SelectMany(stream => stream.Events)
                .Select(e => e.Data)
                .OfType<DomainEvent>()
                .ToList();

            if (domainEvents.Count > 0)
                domainEventCollector.Collect(domainEvents);

            return Task.CompletedTask;
        }
    }
}