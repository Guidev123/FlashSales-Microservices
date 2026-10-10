using FlashSales.Domain.Results;
using FluentValidation;

namespace FlashSales.Application.Extensions
{
    public static class FluentValidationExtensions
    {
        public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(this IRuleBuilderOptions<T, TProperty> ruleBuilder, Error error)
        {
            return ruleBuilder.WithMessage(error.Description);
        }
    }
}