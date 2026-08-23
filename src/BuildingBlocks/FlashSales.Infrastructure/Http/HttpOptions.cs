namespace FlashSales.Infrastructure.Http
{
    public record HttpOptions : HttpResilienceOptions
    {
        public string BaseUrl { get; set; } = default!;
        public string Scope { get; set; } = default!;
    }
}