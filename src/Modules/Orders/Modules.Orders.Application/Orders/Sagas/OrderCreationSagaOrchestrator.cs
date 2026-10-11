using FlashSales.Application.Abstractions;
using FlashSales.Application.FeatureFlags;
using FlashSales.Domain.Results;
using Microsoft.Extensions.Logging;
using Modules.Coupons.Contracts;
using Modules.Launches.Contracts;
using Modules.Orders.Application.Orders.Mappers;
using Modules.Orders.Domain.Orders.Entities;
using Modules.Orders.Domain.Orders.Errors;
using Modules.Orders.Domain.Orders.Models;
using Modules.Orders.Domain.Orders.Repositories;
using Modules.Payments.Contracts;

namespace Modules.Orders.Application.Orders.Sagas
{
    public sealed class OrderCreationSagaOrchestrator(
        ILogger<OrderCreationSaga> logger,
        IOrderRepository orderRepository,
        IOrderCreationSagaRepository sagaRepository,
        IUnitOfWork unitOfWork,
        ILaunchesPublicApi launchesPublicApi,
        IPaymentsPublicApi paymentsPublicApi,
        ICouponsPublicApi couponPublicApi,
        IFeatureFlagService featureFlagService
        )
    {
        public async Task<Result<string>> StartAsync(
            Order order,
            string customerEmail,
            IReadOnlyCollection<CheckoutLineItem> items,
            CancellationToken cancellationToken)
        {
            using var scope = logger.BeginScope(new Dictionary<string, object>()
            {
                ["CustomerEmail"] = customerEmail,
                ["CustomerId"] = order.CustomerId,
                ["Quantity"] = order.Quantity
            });

            return await CreateOrder(order, cancellationToken)
                .Bind(saga => ReserveAsync(order, saga, cancellationToken)
                    .Bind(() => CheckoutAsync(order, customerEmail, items, cancellationToken)
                        .TapError(error => CompensateAsync(
                            saga,
                            order,
                            $"Failed to start checkout: {error.Description}",
                            cancellationToken)))
                    .Tap(_ => CompleteAsync(order, saga, cancellationToken)));
        }

        public async Task<Result> CompensateAsync(Guid sagaId, string reason, CancellationToken cancellationToken)
        {
            var saga = await sagaRepository.GetByIdAsync(sagaId, cancellationToken);
            if (saga is null || saga.Step is OrderCreationSagaStep.Completed or OrderCreationSagaStep.Failed or OrderCreationSagaStep.Compensated)
            {
                return Result.Success();
            }

            var order = await orderRepository.GetByIdAsync(saga.Id, cancellationToken);
            if (order is null)
            {
                return Result.Success();
            }

            await CompensateAsync(saga, order, reason, cancellationToken);

            return Result.Success();
        }

        private async Task CompensateAsync(OrderCreationSaga saga, Order order, string reason, CancellationToken cancellationToken)
        {
            if (saga.Step == OrderCreationSagaStep.InitiatingCheckout)
            {
                saga.RequestCompensation(reason);
                sagaRepository.Update(saga);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                await launchesPublicApi.ReleaseAsync(new(order.LaunchId, order.Quantity, order.Id), cancellationToken);

                saga.MarkCompensated();
            }
            else
            {
                saga.Fail(reason);
            }

            order.Cancel(reason);

            sagaRepository.Update(saga);
            await orderRepository.AppendAsync(order, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<Result<OrderCreationSaga>> CreateOrder(
            Order order,
            CancellationToken cancellationToken
            )
        {
            var saga = OrderCreationSaga.Create(order.Id);

            try
            {
                await orderRepository.StartStreamAsync(order, cancellationToken);
            }
            catch (ActiveOrderAlreadyExistsException ex)
            {
                return Result.Failure<OrderCreationSaga>(OrderErrors.ActiveOrderAlreadyExists(ex.LaunchId));
            }

            sagaRepository.Add(saga);

            var createResult = await unitOfWork.SaveChangesAsync(cancellationToken);
            if (createResult.IsFailure)
            {
                return Result.Failure<OrderCreationSaga>(OrderErrors.ReservationFailed);
            }

            return saga;
        }

        private async Task<Result> ReserveAsync(
            Order order,
            OrderCreationSaga saga,
            CancellationToken cancellationToken
            )
        {
            var reserveResult = await launchesPublicApi.ReserveAsync(new(order.LaunchId, order.Quantity, order.Id), cancellationToken);
            if (reserveResult.IsFailure)
            {
                saga.Fail($"Failed to reserve stock: {reserveResult.Error!.Description}");
                order.Cancel(saga.LastError!);
                sagaRepository.Update(saga);
                await orderRepository.AppendAsync(order, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Failure(reserveResult.Error!);
            }

            saga.MarkStockReserved();
            sagaRepository.Update(saga);

            var checkpointResult = await unitOfWork.SaveChangesAsync(cancellationToken);
            if (checkpointResult.IsFailure)
            {
                return Result.Failure(OrderErrors.ReservationFailed);
            }

            return Result.Success();
        }

        private async Task<Result<string>> CheckoutAsync(
            Order order,
            string customerEmail,
            IReadOnlyCollection<CheckoutLineItem> items,
            CancellationToken cancellationToken)
        {
            var result = await paymentsPublicApi.InitiateCheckoutAsync(
                PaymentMappers.MapToRequest(
                    order.Id, order.OrderCode, order.TotalAmount,
                    order.CustomerId, customerEmail, items),
                cancellationToken);

            return result.Map(checkout => checkout.CheckoutUrl);
        }

        private async Task ApplyCouponAsync(
            Order order,
            OrderCreationSaga saga,
            CancellationToken cancellationToken
            )
        {
            var canApply = await CanApplyCouponAsync(cancellationToken);
            if (!canApply)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("The discount coupon feature is disabled for the customer {CustomerId}", order.CustomerId);
                }
            }
        }

        private Task<bool> CanApplyCouponAsync(CancellationToken cancellationToken)
        {
            return featureFlagService.IsEnabledAsync("Coupons", cancellationToken);
        }

        private async Task CompleteAsync(Order order, OrderCreationSaga saga, CancellationToken cancellationToken)
        {
            saga.MarkCompleted();
            order.MarkPaymentProcessing();
            sagaRepository.Update(saga);
            await orderRepository.AppendAsync(order, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}