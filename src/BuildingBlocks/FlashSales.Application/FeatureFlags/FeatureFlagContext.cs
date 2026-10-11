namespace FlashSales.Application.FeatureFlags
{
    public sealed record FeatureFlagContext(string? UserId, IReadOnlyCollection<string>? Groups = null);
}
