namespace LinkedIn.Modules.Users.Domain.Enums;

/// <summary>
/// A plain enum column rather than full ASP.NET Identity Roles (IdentityRole,
/// UserRole join table). Two fixed roles don't need a whole role-management
/// system - revisit only if roles ever need to be dynamic/admin-editable.
/// </summary>
public enum UserRole
{
    Candidate,
    Employer
}
