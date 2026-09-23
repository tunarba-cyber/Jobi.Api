using LinkedIn.Modules.Users.Features.ConfirmEmail;
using LinkedIn.Modules.Users.Features.Dtos;
using LinkedIn.Modules.Users.Features.ForgotPassword;
using LinkedIn.Modules.Users.Features.Login;
using LinkedIn.Modules.Users.Features.Logout;
using LinkedIn.Modules.Users.Features.LogoutAll;
using LinkedIn.Modules.Users.Features.Register;
using System.Security.Claims;
using LinkedIn.Modules.Users.Features.RefreshToken;
using LinkedIn.Modules.Users.Features.ResetPassword;
using LinkedIn.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;

namespace LinkedIn.Modules.Users.Features;

internal static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/auth")
            .WithTags("Auth")
            ;

        group.MapPost("/register", Register)
            .WithName("Register")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireRateLimiting("auth");

        group.MapPost("/confirm-email", ConfirmEmail)
            .WithName("ConfirmEmail")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/login", Login)
            .WithName("Login")
            .Produces<AuthResultDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireRateLimiting("auth");

        group.MapPost("/refresh", Refresh)
            .WithName("RefreshToken")
            .Produces<AuthResultDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/forgot-password", ForgotPassword)
            .WithName("ForgotPassword")
            .WithSummary("Always returns 204, whether or not the email exists - prevents account enumeration.")
            .Produces(StatusCodes.Status204NoContent)
            .RequireRateLimiting("auth");

        group.MapPost("/reset-password", ResetPassword)
            .WithName("ResetPassword")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", Logout)
            .WithName("Logout")
            .Produces(StatusCodes.Status204NoContent);

        // Any authenticated user may log out ALL of their own sessions - no
        // role requirement, just needs to be a valid token for SOME account.
        group.MapPost("/logout-all", LogoutAll)
            .WithName("LogoutAll")
            .WithSummary("Revokes every active refresh token for the current user - all devices/sessions.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();
    }

    private static async Task<IResult> Register(RegisterCommand command, ISender sender, CancellationToken ct) =>
        (await sender.Send(command, ct)).ToHttpResult();

    private static async Task<IResult> ConfirmEmail(ConfirmEmailCommand command, ISender sender, CancellationToken ct) =>
        (await sender.Send(command, ct)).ToHttpResult();

    private static async Task<IResult> Login(LoginCommand command, ISender sender, CancellationToken ct) =>
        (await sender.Send(command, ct)).ToHttpResult();

    private static async Task<IResult> Refresh(RefreshTokenRequest request, ISender sender, CancellationToken ct) =>
        (await sender.Send(new RefreshTokenCommand(request.RefreshToken), ct)).ToHttpResult();

    private static async Task<IResult> ForgotPassword(ForgotPasswordCommand command, ISender sender, CancellationToken ct) =>
        (await sender.Send(command, ct)).ToHttpResult();

    private static async Task<IResult> ResetPassword(ResetPasswordCommand command, ISender sender, CancellationToken ct) =>
        (await sender.Send(command, ct)).ToHttpResult();

    private static async Task<IResult> Logout(LogoutCommand command, ISender sender, CancellationToken ct) =>
        (await sender.Send(command, ct)).ToHttpResult();

    private static async Task<IResult> LogoutAll(ClaimsPrincipal user, ISender sender, CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!; // guaranteed present - endpoint requires auth
        return (await sender.Send(new LogoutAllCommand(userId), ct)).ToHttpResult();
    }

    // A tiny wrapper so the request body's field is named "refreshToken" rather
    // than colliding with the command type's own name in the request body JSON.
    private sealed record RefreshTokenRequest(string RefreshToken);
}
