using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Payments.Application.Payments.DTOs;
using Modules.Payments.Application.Payments.Features.Checkout;
using System.Security.Claims;
using static Modules.Payments.Contracts.IPaymentsPublicApi;

namespace Modules.Payments.Endpoints.Payments
{
    internal sealed class CheckoutPaymentEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v1/payments/checkout", async (
                Request request,
                ISender sender,
                ClaimsPrincipal claimsPrincipal,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new CheckoutPaymentCommand(
                    request.OrderId,
                    request.OrderCode,
                    request.Amount,
                    claimsPrincipal.GetUserId(),
                    claimsPrincipal.GetEmail(),
                    request.Items
                ), cancellationToken);

                return result.Match(() => Results.Ok(new InitiateCheckoutResponse(
                    result.Value.PaymentId,
                    result.Value.AttemptId,
                    result.Value.CheckoutUrl)
                    ), ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
            .RequirePermission(PaymentsPermissions.Checkout)
            .RequireScope(PaymentsScopes.Write);
        }

        record Request(
            decimal Amount,
            string OrderCode,
            Guid OrderId,
            List<PaymentCheckoutLineItem> Items
            );
    }
}