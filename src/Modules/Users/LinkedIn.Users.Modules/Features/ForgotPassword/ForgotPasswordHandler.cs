using System.Text;
using LinkedIn.Modules.Users.Domain.Entities;
using LinkedIn.Modules.Users.Infrastructure.Email;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace LinkedIn.Modules.Users.Features.ForgotPassword;

internal sealed class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailSender _emailSender;

    public ForgotPasswordHandler(UserManager<AppUser> userManager, IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // No user found, or found but email never confirmed: still return
        // Success with no email sent. The caller cannot distinguish "sent" from
        // "no such account" - that is the entire point of this check.
        if (user is null || !await _userManager.IsEmailConfirmedAsync(user))
            return Result.Success();

        var rawToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

        var body = $$"""
            <p>Reset your password by calling <code>POST /api/auth/reset-password</code> with:</p>
            <pre>{ "email": "{{user.Email}}", "token": "{{encodedToken}}", "newPassword": "..." }</pre>
            <p>If you didn't request this, you can ignore this email.</p>
            """;

        await _emailSender.SendAsync(user.Email!, "Reset your password", body, cancellationToken);

        return Result.Success();
    }
}
