using FlashSales.Infrastructure.Database;

namespace Modules.Coupons.Infrastructure.Database.Repositories
{
    internal sealed class UnitOfWork(CouponsDbContext context)
        : BaseUnitOfWork<CouponsDbContext>(context);
}