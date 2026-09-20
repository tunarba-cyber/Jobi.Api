namespace LinkedIn.Shared.Infrastructure.Persistence;

/// <summary>
/// Abstracts "who is making this request" so the auditing interceptor can stamp
/// CreatedBy/ModifiedBy without EF knowing anything about HTTP.
/// </summary>
public interface ICurrentUser
{
    string? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
}
