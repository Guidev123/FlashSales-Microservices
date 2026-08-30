using Modules.Orders.Domain.Orders.Entities;

namespace Modules.Orders.Domain.Orders.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task StartStreamAsync(Order order, CancellationToken cancellationToken = default);

        Task AppendAsync(Order order, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<Guid>> GetStaleAwaitingOrProcessingAsync(CancellationToken cancellationToken = default);
    }
}