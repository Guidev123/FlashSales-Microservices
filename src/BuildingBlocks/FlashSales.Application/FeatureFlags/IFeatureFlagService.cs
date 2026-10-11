namespace FlashSales.Application.FeatureFlags
{
    public interface IFeatureFlagService
    {
        Task<bool> IsEnabledAsync(string feature, CancellationToken cancellationToken = default);

        Task<bool> IsEnabledAsync(string feature, FeatureFlagContext context, CancellationToken cancellationToken = default);
    }
}