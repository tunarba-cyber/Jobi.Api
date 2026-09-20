using System.Security.Claims;
using LinkedIn.Shared.Infrastructure.Persistence;

namespace LinkedIn.Api.Extensions;

/// <summary>HTTP-backed implementation of ICurrentUser used by the audit interceptor.</summary>
internal sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    public string? UserId => _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? UserName => _accessor.HttpContext?.User.Identity?.Name;
    public bool IsAuthenticated => _accessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
