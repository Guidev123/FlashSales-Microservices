namespace FlashSales.Infrastructure.Extensions
{
    internal sealed class AzureConnectionString
    {
        private readonly Dictionary<string, string> _parts;

        private AzureConnectionString(Dictionary<string, string> parts) => _parts = parts;

        public static AzureConnectionString Parse(string connectionString)
        {
            var parts = connectionString
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(part => part.Split('=', 2))
                .Where(pair => pair.Length == 2)
                .ToDictionary(pair => pair[0].Trim(), pair => pair[1].Trim(), StringComparer.OrdinalIgnoreCase);

            return new AzureConnectionString(parts);
        }

        public Uri Endpoint
            => _parts.TryGetValue("Endpoint", out var endpoint)
                ? new Uri(endpoint)
                : throw new InvalidOperationException("The connection string has no 'Endpoint'.");

        public bool HasCredentials(params string[] credentialKeys)
            => credentialKeys.Any(key => _parts.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value));
    }
}
