namespace FlashSales.Application.Outbox
{
    public sealed class OutboxOptions
    {
        public const string SectionName = "Outbox";

        public int IntervalInSeconds { get; set; }
        public int BatchSize { get; set; }
        public int MaxRetryCount { get; set; }

        public bool AutoRequeueEnabled { get; set; } = false;
        public int AutoRequeueIntervalInSeconds { get; set; } = 3600;
    }
}