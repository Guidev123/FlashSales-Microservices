namespace FlashSales.Infrastructure.Http
{
    public sealed record ClientCredentialsOptions
    {
        public const string SectionName = "ClientCredentials";

        public string Authority { get; set; } = default!;
        public string ClientId { get; set; } = default!;
        public string ClientSecret { get; set; } = default!;
    }
}
