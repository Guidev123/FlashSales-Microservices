using FlashSales.Infrastructure.Database;

namespace Modules.Orders.Infrastructure.Database.Repositories
{
    internal sealed class UnitOfWork(OrdersDbContext context)
        : BaseUnitOfWork<OrdersDbContext>(context);
}
