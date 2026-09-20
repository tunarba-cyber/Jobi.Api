using System.Text;
using LinkedIn.Modules.Users.Domain.Entities;
using LinkedIn.Modules.Users.Domain.Errors;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace LinkedIn.Modules.Users.Features.ConfirmEmail;

internal sealed class ConfirmEmailHandler : IRequestHandler<ConfirmEmailCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;

    public ConfirmEmailHandler(UserManager<AppUser> userManager) => _userManager = userManager;

    public async Task<Result> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
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

        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        return result.Succeeded
            ? Result.Success()
            : Result.Failure(UserErrors.InvalidOrExpiredToken);
    }
}
