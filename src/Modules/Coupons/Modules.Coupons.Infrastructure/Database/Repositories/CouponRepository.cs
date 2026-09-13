using Microsoft.EntityFrameworkCore;
using Modules.Coupons.Domain.Coupons.Entities;
using Modules.Coupons.Domain.Coupons.Repositories;

namespace Modules.Coupons.Infrastructure.Database.Repositories
{
    internal sealed class CouponRepository(CouponsDbContext context) : ICouponRepository
    {
        public void Add(Coupon coupon)
        {
            context.Coupons.Add(coupon);
        }

        public Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default)
        {
            return context.Coupons.AsNoTracking().AnyAsync(c => c.Code == code, cancellationToken);
        }

        public Task<Coupon?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return context.Coupons.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public void Update(Coupon coupon)
        {
            context.Coupons.Update(coupon);
        }
    }
}