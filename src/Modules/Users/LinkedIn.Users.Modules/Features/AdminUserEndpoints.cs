using System.Security.Claims;
using LinkedIn.Modules.Users.Domain.Entities;
using LinkedIn.Modules.Users.Domain.Enums;
using LinkedIn.Modules.Users.Infrastructure.Persistence;
using LinkedIn.Shared.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Users.Features;

public sealed record AdminUserDto(string Id, string? Email, string FirstName, string LastName, UserRole Role, bool IsSuspended, bool EmailConfirmed, DateTimeOffset CreatedAtUtc);

internal static class AdminUserEndpoints
{
    private sealed record ChangeRoleRequest(int Role);

    public static void MapAdminUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin/users").WithTags("AdminUsers").RequireAuthorization("RequireAdmin");
        group.MapGet("/", GetUsers);
        group.MapPut("/{id}/suspend", Suspend);
        group.MapPut("/{id}/reinstate", Reinstate);
        group.MapPut("/{id}/role", ChangeRole);
    }

    private static async Task<IResult> GetUsers(UsersDbContext db, CancellationToken ct,
        string? search = null, UserRole? role = null, int page = 1, int pageSize = 15)
    {
        var now = DateTimeOffset.UtcNow;
        var s = search?.Trim();

        var query = db.Users.AsNoTracking()
            .WhereIf(!string.IsNullOrWhiteSpace(s), u => u.Email!.Contains(s!) || u.FirstName.Contains(s!) || u.LastName.Contains(s!))
            .WhereIf(role.HasValue, u => u.Role == role)
            .OrderByDescending(u => u.CreatedAtUtc)
            .Select(u => new AdminUserDto(u.Id, u.Email, u.FirstName, u.LastName, u.Role,
                u.LockoutEnd != null && u.LockoutEnd > now, u.EmailConfirmed, u.CreatedAtUtc));

        return Results.Ok(await query.ToPagedResultAsync(Math.Max(page, 1), Math.Clamp(pageSize, 1, 50), ct));
    }

    private static async Task<IResult> Suspend(string id, ClaimsPrincipal caller, UserManager<AppUser> um, UsersDbContext db, CancellationToken ct)
    {
        if (IsSelf(caller, id)) return Bad("You can't suspend your own account.");
        var user = await um.FindByIdAsync(id);
        if (user is null) return Results.NotFound();

        await um.SetLockoutEnabledAsync(user, true);
        await um.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        await RevokeSessionsAsync(db, id, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> Reinstate(string id, UserManager<AppUser> um)
    {
        var user = await um.FindByIdAsync(id);
        if (user is null) return Results.NotFound();

        await um.SetLockoutEndDateAsync(user, null);
        await um.ResetAccessFailedCountAsync(user);
        return Results.NoContent();
    }

    private static async Task<IResult> ChangeRole(string id, ChangeRoleRequest request, ClaimsPrincipal caller, UserManager<AppUser> um, UsersDbContext db, CancellationToken ct)
    {
        if (IsSelf(caller, id)) return Bad("You can't change your own role.");
        if (!Enum.IsDefined(typeof(UserRole), request.Role)) return Bad("Unknown role.");

        var user = await um.FindByIdAsync(id);
        if (user is null) return Results.NotFound();

        user.Role = (UserRole)request.Role;
        var result = await um.UpdateAsync(user);
        if (!result.Succeeded) return Bad(string.Join(" ", result.Errors.Select(e => e.Description)));

        await RevokeSessionsAsync(db, id, ct); // forces re-login so the new role lands in the JWT
        return Results.NoContent();
    }

    private static bool IsSelf(ClaimsPrincipal caller, string id) =>
        caller.FindFirstValue(ClaimTypes.NameIdentifier) == id;

    private static IResult Bad(string detail) =>
        Results.Problem(title: "User.Invalid", detail: detail, statusCode: StatusCodes.Status400BadRequest);

    private static Task RevokeSessionsAsync(UsersDbContext db, string userId, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        return db.RefreshTokens
            .Where(t => t.AppUserId == userId && t.RevokedAtUtc == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAtUtc, now), ct);
    }
}