using FlashSales.Domain.Results;
using Modules.Coupons.Contracts.Requests;
using Modules.Coupons.Contracts.Responses;

namespace Modules.Coupons.Contracts
{
    public interface ICouponsPublicApi
    {
        Task<Result<RedeemResponse>> RedeemAsync(
            RedeemRequest request,
            CancellationToken cancellationToken = default
            );

        Task<Result> ReleaseAsync(
            ReleaseRequest request,
            CancellationToken cancellationToken = default
            );
    }
}