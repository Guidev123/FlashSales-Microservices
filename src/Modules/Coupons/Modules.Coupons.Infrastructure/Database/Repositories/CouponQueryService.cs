using Dapper;
using FlashSales.Application.Abstractions;
using Modules.Coupons.Application.Coupons.DTOs;
using Modules.Coupons.Application.Coupons.Services;
using Modules.Coupons.Domain.Coupons.Enums;

namespace Modules.Coupons.Infrastructure.Database.Repositories
{
    internal sealed class CouponQueryService(IUnitOfWork unitOfWork) : ICouponQueryService
    {
        public Task<CouponResponse?> GetByCodeAsync(
            string code,
            CouponStatus couponStatus = CouponStatus.Active,
            CancellationToken cancellationToken = default
            )
        {
            const string sql = """
                SELECT
                    c."Id",
                    c."Code",
                    c."Type",
                    c."DiscountValue" AS Value,
                    c."MaxDiscountAmount" AS MaxValue,
                    c."MaxRedemptions" - c."RedeemedCount" AS AvailableRedemptions,
                    c."ValidUntil" AS ExpirationDate
                FROM coupons."Coupons" AS c
                WHERE c."Code" = @code
                AND c."Status" = @couponStatus
                """;

            return unitOfWork.Connection.QueryFirstOrDefaultAsync<CouponResponse>(sql, new { code, couponStatus });
        }

        public Task<CouponResponse?> GetByIdAsync(
            Guid id,
            CouponStatus couponStatus = CouponStatus.Active,
            CancellationToken cancellationToken = default
            )
        {
            const string sql = """
                SELECT
                    c."Id",
                    c."Code",
                    c."Type",
                    c."DiscountValue" AS Value,
                    c."MaxDiscountAmount" AS MaxValue,
                    c."MaxRedemptions" - c."RedeemedCount" AS AvailableRedemptions,
                    c."ValidUntil" AS ExpirationDate
                FROM coupons."Coupons" AS c
                WHERE c."Id" = @id
                AND c."Status" = @couponStatus
                """;

            return unitOfWork.Connection.QueryFirstOrDefaultAsync<CouponResponse>(sql, new { id, couponStatus });
        }
    }
}