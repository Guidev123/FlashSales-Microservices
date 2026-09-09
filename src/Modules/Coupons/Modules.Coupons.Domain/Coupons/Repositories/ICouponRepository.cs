using Modules.Coupons.Domain.Coupons.Entities;

namespace Modules.Coupons.Domain.Coupons.Repositories
{
    public interface ICouponRepository
    {
        Task<Coupon?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default);

        void Add(Coupon coupon);

        void Update(Coupon coupon);
    }
}