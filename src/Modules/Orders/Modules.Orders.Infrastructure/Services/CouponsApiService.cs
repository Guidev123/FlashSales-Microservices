using FlashSales.Domain.Results;
using Modules.Coupons.Contracts;
using Modules.Coupons.Contracts.Requests;
using Modules.Coupons.Contracts.Responses;

namespace Modules.Orders.Infrastructure.Services
{
    internal sealed class CouponsApiService : ICouponsPublicApi
    {
        public Task<Result<RedeemResponse>> RedeemAsync(RedeemRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Result> ReleaseAsync(ReleaseRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}