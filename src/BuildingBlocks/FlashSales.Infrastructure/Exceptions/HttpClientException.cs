namespace FlashSales.Infrastructure.Exceptions
{
    public abstract class HttpClientException : Exception
    {
        public string? RequestUri { get; }

        protected HttpClientException(string message, string? requestUri, Exception? inner = null)
            : base(message, inner) => RequestUri = requestUri;
    }

    public sealed class HttpTimeoutException : HttpClientException
    {
        public HttpTimeoutException(string? requestUri, Exception inner)
            : base($"Timeout to call API {requestUri}", requestUri, inner) { }
    }

    public sealed class HttpTransportException : HttpClientException
    {
        public HttpTransportException(string? requestUri, Exception inner)
            : base($"Transport error when calling API {requestUri}", requestUri, inner) { }
    }

    public sealed class HttpApiException : HttpClientException
    {
        public int StatusCode { get; }
        public string? ResponseBody { get; }

        public HttpApiException(string? requestUri, int statusCode, string? responseBody)
            : base($"API call to {requestUri} returned {statusCode}", requestUri)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }
}