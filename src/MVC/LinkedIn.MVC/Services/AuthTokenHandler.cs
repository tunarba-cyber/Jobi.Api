using System.Net.Http.Headers;
using LinkedIn.MVC.Models.Api;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace LinkedIn.MVC.Services;

/// <summary>
/// Runs before every request the typed IJobiApiClient sends. Reads the JWT
/// stored inside the auth cookie's properties, attaches it as a Bearer header,
/// and transparently refreshes it if it's expired - callers never see any of this.
/// </summary>
public sealed class AuthTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _accessor;
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthTokenHandler(IHttpContextAccessor accessor, IHttpClientFactory httpClientFactory)
    {
        _accessor = accessor;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var httpContext = _accessor.HttpContext;
        if (httpContext is null)
        {
            return await base.SendAsync(request, ct); // background call, no signed-in user to attach
        }

        var authResult = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!authResult.Succeeded || authResult.Properties is null)
        {
            return await base.SendAsync(request, ct); // anonymous - endpoint will 401 if it needed auth
        }

        var accessToken = authResult.Properties.GetTokenValue("access_token");
        var expiresAtRaw = authResult.Properties.GetTokenValue("expires_at");
        var refreshToken = authResult.Properties.GetTokenValue("refresh_token");

        var expiresAt = DateTimeOffset.TryParse(expiresAtRaw, out var parsed) ? parsed : DateTimeOffset.MinValue;

        // Refresh a little before actual expiry, not after - avoids a request
        // failing mid-flight right at the boundary.
        if (accessToken is not null && refreshToken is not null && expiresAt < DateTimeOffset.UtcNow.AddSeconds(30))
        {
            var refreshed = await TryRefreshAsync(refreshToken, ct);
            if (refreshed is not null)
            {
                accessToken = refreshed.AccessToken;

                authResult.Properties.UpdateTokenValue("access_token", refreshed.AccessToken);
                authResult.Properties.UpdateTokenValue("refresh_token", refreshed.RefreshToken);
                authResult.Properties.UpdateTokenValue("expires_at", refreshed.AccessTokenExpiresAtUtc.ToString("o"));

                await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, authResult.Principal!, authResult.Properties);
            }
            else
            {
                // Refresh token itself is dead - sign the user out cleanly rather
                // than silently sending a request that's going to 401 anyway.
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                accessToken = null;
            }
        }

        if (accessToken is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return await base.SendAsync(request, ct);
    }

    private async Task<AuthResultDto?> TryRefreshAsync(string refreshToken, CancellationToken ct)
    {
        try
        {
            // Deliberately a separate named client with NO AuthTokenHandler
            // attached - calling refresh through this same handler would recurse.
            var rawClient = _httpClientFactory.CreateClient("JobiApiRaw");
            using var response = await rawClient.PostAsJsonAsync("api/auth/refresh", new { refreshToken }, ct);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<AuthResultDto>(cancellationToken: ct)
                : null;
        }
        catch (Exception) { return null; }
    }
}