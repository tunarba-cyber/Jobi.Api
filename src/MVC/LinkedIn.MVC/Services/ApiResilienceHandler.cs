using System.Net;
using System.Text;

namespace LinkedIn.MVC.Services;

/// <summary>Turns "can't reach the API" into an ordinary 503, so no call site can crash on an outage.</summary>
public sealed class ApiResilienceHandler : DelegatingHandler
{
    private readonly ILogger<ApiResilienceHandler> _logger;
    public ApiResilienceHandler(ILogger<ApiResilienceHandler> logger) => _logger = logger;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        try
        {
            return await base.SendAsync(request, ct);
        }
        catch (Exception ex) when (!ct.IsCancellationRequested && (ex is HttpRequestException or TaskCanceledException))
        {
            _logger.LogWarning(ex, "API unreachable: {Method} {Uri}", request.Method, request.RequestUri);
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                RequestMessage = request,
                Content = new StringContent(
                    "{\"title\":\"Api.Unavailable\",\"detail\":\"The service is temporarily unavailable. Please try again in a moment.\"}",
                    Encoding.UTF8, "application/problem+json")
            };
        }
    }
}