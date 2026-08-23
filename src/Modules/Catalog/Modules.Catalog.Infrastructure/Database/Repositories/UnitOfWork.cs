using FlashSales.Infrastructure.Database;

namespace Modules.Catalog.Infrastructure.Database.Repositories
{
    internal sealed class UnitOfWork(CatalogDbContext context)
        : BaseUnitOfWork<CatalogDbContext>(context);
}
