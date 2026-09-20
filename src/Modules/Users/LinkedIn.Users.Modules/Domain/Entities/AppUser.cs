using LinkedIn.Modules.Users.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace LinkedIn.Modules.Users.Domain.Entities;

/// <summary>
/// Password hashing, lockout, email-confirmation tokens and password-reset
/// tokens all come from IdentityUser/UserManager - none of that is
/// hand-rolled here. FirstName/LastName/Role are the only additions this
/// app actually needs on top of it.
/// </summary>
public sealed class AppUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
}
