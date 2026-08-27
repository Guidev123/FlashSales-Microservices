using FlashSales.Infrastructure.Http;

namespace Modules.Orders.Infrastructure.Options
{
    internal sealed record ApiOptions
    {
        public const string SectionName = "ApiOptions";

        public HttpOptions LaunchesApi { get; set; } = null!;
        public HttpOptions PaymentsApi { get; set; } = null!;
        public HttpOptions UsersApi { get; set; } = null!;
    }
}