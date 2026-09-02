using MidR.Interfaces;
using Modules.AnalyticsCollector.Core.Models;
using System.Reactive.Subjects;

namespace Modules.AnalyticsCollector.Core
{
    internal sealed class AnalyticsRequestHandler(Subject<AnalyticsRequest> subject) : INotificationHandler<AnalyticsRequest>
    {
        public async Task ExecuteAsync(AnalyticsRequest notification, CancellationToken cancellationToken)
        {
            subject.OnNext(notification);
        }
    }
}