using FlashSales.Infrastructure.Exceptions;

namespace FlashSales.Infrastructure.Http
{
    public sealed class ExceptionTranslationDelegatingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                return await base.SendAsync(request, cancellationToken);
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw new HttpTimeoutException(request.RequestUri?.ToString(), ex);
            }
            catch (HttpRequestException ex)
            {
                throw new HttpTransportException(request.RequestUri?.ToString(), ex);
            }
        }
    }
}