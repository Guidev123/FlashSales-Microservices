using FlashSales.Infrastructure.Database;

namespace Modules.Launches.Infrastructure.Database.Repositories
{
    internal sealed class UnitOfWork(LaunchesDbContext context)
        : BaseUnitOfWork<LaunchesDbContext>(context);
}
