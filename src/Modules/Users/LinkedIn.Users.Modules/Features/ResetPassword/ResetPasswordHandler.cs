using System.Text;
using LinkedIn.Modules.Users.Domain.Entities;
using LinkedIn.Modules.Users.Domain.Errors;
using LinkedIn.Modules.Users.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Users.Features.ResetPassword;

internal sealed class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly UsersDbContext _db;
    private readonly TimeProvider _timeProvider;

    public ResetPasswordHandler(UserManager<AppUser> userManager, UsersDbContext db, TimeProvider timeProvider)
    {
        _userManager = userManager;
        _db = db;
        _timeProvider = timeProvider;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result.Failure(UserErrors.InvalidOrExpiredToken);

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
        }
        catch (FormatException)
        {
            return Result.Failure(UserErrors.InvalidOrExpiredToken);
        }

        var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);
        if (!result.Succeeded)
        {
            var details = string.Join("; ", result.Errors.Select(e => e.Description));
            return Result.Failure(UserErrors.PasswordResetFailed(details));
        }

        // Password just changed - force every existing session to re-login
        // rather than leaving old refresh tokens usable.
        var activeTokens = await _db.RefreshTokens
            .Where(t => t.AppUserId == user.Id && t.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        var now = _timeProvider.GetUtcNow();
        foreach (var token in activeTokens)
            token.RevokedAtUtc = now;

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
