using FlashSales.Infrastructure.Database;

namespace Modules.Users.Infrastructure.Database.Repositories
{
    internal sealed class UnitOfWork(UsersDbContext context)
        : BaseUnitOfWork<UsersDbContext>(context);
}
