using MidR.Interfaces;

namespace Modules.AnalyticsCollector.Core.Models
{
    internal sealed record AnalyticsRequest(
        string EventType,
        string SessionId,
        Guid? UserId,
        Guid? LaunchId,
        string PageUrl,
        string? ElementId,
        DateTimeOffset OccurredAt,
        string? Referrer,
        string? UserAgent,
        Dictionary<string, string>? Metadata
        ) : INotification;
}