using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Payments.Application.Payments.Features.Checkout;
using static Modules.Payments.Contracts.IPaymentsPublicApi;

namespace Modules.Payments.Endpoints.Payments
{
    internal sealed class CheckoutPaymentEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/payments/checkout", async (
                CheckoutPaymentCommand request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(request, cancellationToken);

                return result.Match(() => Results.Ok(new InitiateCheckoutResponse(
                    result.Value.PaymentId,
                    result.Value.AttemptId,
                    result.Value.CheckoutUrl)
                    ), ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
            .RequirePermission(PaymentsPermissions.Checkout)
            .RequireScope(PaymentsScopes.Write);
        }
    }
}