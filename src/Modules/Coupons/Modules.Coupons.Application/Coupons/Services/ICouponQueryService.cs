using Modules.Coupons.Application.Coupons.DTOs;
using Modules.Coupons.Domain.Coupons.Enums;

namespace Modules.Coupons.Application.Coupons.Services
{
    public interface ICouponQueryService
    {
        Task<CouponResponse?> GetByCodeAsync(
            string code,
            CouponStatus couponStatus = CouponStatus.Active,
            CancellationToken cancellationToken = default
            );

        Task<CouponResponse?> GetByIdAsync(
            Guid id,
            CouponStatus couponStatus = CouponStatus.Active,
            CancellationToken cancellationToken = default
            );
    }
}