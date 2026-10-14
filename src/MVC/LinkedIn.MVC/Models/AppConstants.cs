namespace LinkedIn.MVC.Models;

public static class AppRoles
{
    public const string Candidate = "Candidate";
    public const string Employer = "Employer";
    public const string Admin = "Admin";
    public const string CandidateOrEmployer = Candidate + "," + Employer;
}

public static class AuthTokens
{
    public const string Access = "access_token";
    public const string Refresh = "refresh_token";
    public const string ExpiresAt = "expires_at";
}