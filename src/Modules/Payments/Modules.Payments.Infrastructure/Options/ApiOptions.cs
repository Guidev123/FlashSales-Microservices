using FlashSales.Infrastructure.Http;

namespace Modules.Payments.Infrastructure.Options
{
    internal sealed record ApiOptions
    {
        public const string SectionName = "ApiOptions";

        public HttpOptions UsersApi { get; set; } = null!;
    }
}
