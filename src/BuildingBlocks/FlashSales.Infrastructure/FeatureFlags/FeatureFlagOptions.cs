namespace FlashSales.Infrastructure.FeatureFlags
{
    public sealed class FeatureFlagOptions
    {
        public const string SectionName = "FeatureFlags";
        public string? Label { get; set; }
        public int RefreshIntervalInSeconds { get; set; } = 30;
    }
}
