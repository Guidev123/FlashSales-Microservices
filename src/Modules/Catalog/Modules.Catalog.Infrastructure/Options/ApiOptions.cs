using FlashSales.Infrastructure.Http;

namespace Modules.Catalog.Infrastructure.Options
{
    internal sealed record ApiOptions
    {
        public const string SectionName = "ApiOptions";

        public HttpOptions UsersApi { get; set; } = null!;
    }
}
