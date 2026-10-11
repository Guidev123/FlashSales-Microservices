using FlashSales.Application.FeatureFlags;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;

namespace FlashSales.Infrastructure.FeatureFlags
{
    internal sealed class FeatureFlagService(
        IFeatureManager featureManager,
        ILogger<FeatureFlagService> logger
        ) : IFeatureFlagService
    {
        public Task<bool> IsEnabledAsync(string feature, CancellationToken cancellationToken = default)
            => EvaluateAsync(feature, () => featureManager.IsEnabledAsync(feature, cancellationToken));

        public Task<bool> IsEnabledAsync(string feature, FeatureFlagContext context, CancellationToken cancellationToken = default)
        {
            var targetingContext = new TargetingContext
            {
                UserId = context.UserId,
                Groups = context.Groups ?? []
            };

            return EvaluateAsync(feature, () => featureManager.IsEnabledAsync(feature, targetingContext));
        }

        private async Task<bool> EvaluateAsync(string feature, Func<Task<bool>> evaluate)
        {
            try
            {
                return await evaluate();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to evaluate feature flag {Feature}, treating it as disabled", feature);

                return false;
            }
        }
    }
}