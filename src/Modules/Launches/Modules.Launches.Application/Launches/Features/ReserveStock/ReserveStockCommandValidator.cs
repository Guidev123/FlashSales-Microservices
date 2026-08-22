using FluentValidation;
using Modules.Launches.Domain.Launches.Errors;

namespace Modules.Launches.Application.Launches.Features.ReserveStock
{
    internal sealed class ReserveStockCommandValidator : AbstractValidator<ReserveStockCommand>
    {
        public ReserveStockCommandValidator()
        {
            RuleFor(x => x.LaunchId)
                .NotEqual(Guid.Empty)
                .WithMessage(LaunchErrors.LaunchIdCannotBeEmpty.Description);

            RuleFor(x => x.OrderId)
                .NotEqual(Guid.Empty)
                .WithMessage(LaunchErrors.OrderIdCannotBeEmpty.Description);

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(1)
                .WithMessage(LaunchErrors.QuantityMustBeAtLeastOne.Description);
        }
    }
}