using FlashSales.Infrastructure.Database;

namespace Modules.Payments.Infrastructure.Database.Repositories
{
    internal sealed class UnitOfWork(PaymentsDbContext context)
        : BaseUnitOfWork<PaymentsDbContext>(context);
}
