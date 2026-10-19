using System.Text;
using LinkedIn.Modules.Users.Domain.Entities;
using LinkedIn.Modules.Users.Domain.Errors;
using LinkedIn.Modules.Users.Infrastructure;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace LinkedIn.Modules.Users.Features.ConfirmEmail;

internal sealed class ConfirmEmailHandler : IRequestHandler<ConfirmEmailCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ConfirmationEmailService _confirmation;

    public ConfirmEmailHandler(UserManager<AppUser> userManager, ConfirmationEmailService confirmation)
    { _userManager = userManager; _confirmation = confirmation; }

    public async Task<Result> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null) return Result.Failure(UserErrors.InvalidOrExpiredToken);

        if (user.EmailConfirmed) return Result.Success();   // opening a used link again is fine

        if (_confirmation.Verify(user, request.Token))
        {
            user.EmailConfirmed = true;
            var saved = await _userManager.UpdateAsync(user);
            return saved.Succeeded ? Result.Success() : Result.Failure(UserErrors.InvalidOrExpiredToken);
        }

        // Links emailed before this change carry an Identity token.
        try
        {
            var legacy = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            var result = await _userManager.ConfirmEmailAsync(user, legacy);
            return result.Succeeded ? Result.Success() : Result.Failure(UserErrors.InvalidOrExpiredToken);
        }
        catch (FormatException)
        {
            return Result.Failure(UserErrors.InvalidOrExpiredToken);
        }
    }
}