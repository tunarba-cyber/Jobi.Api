namespace LinkedIn.Shared.Abstractions.Security;

public static class AppRoles
{
    public const string Candidate = "Candidate";
    public const string Employer = "Employer";
    public const string Admin = "Admin";
}

public static class AuthPolicies
{
    public const string Candidate = "RequireCandidate";
    public const string Employer = "RequireEmployer";
    public const string Admin = "RequireAdmin";
}

public static class RateLimitPolicies
{
    public const string Auth = "auth";
    public const string Contact = "contact";
    public const string Ai = "ai";
}