namespace LinkedIn.MVC.Services;

public sealed class ForwardClientIpHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _accessor;
    public ForwardClientIpHandler(IHttpContextAccessor accessor) => _accessor = accessor;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var ip = _accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        if (ip is not null) request.Headers.TryAddWithoutValidation("X-Forwarded-For", ip);
        return base.SendAsync(request, ct);
    }
}