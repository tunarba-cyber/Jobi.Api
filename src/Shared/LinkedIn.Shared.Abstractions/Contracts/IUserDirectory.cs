namespace LinkedIn.Shared.Abstractions.Contracts;

public sealed record UserSummary(string Id, string FirstName, string LastName);

/// <summary>
/// Public contract the Users module exposes to other modules. Messaging never
/// touches UsersDbContext or the AppUser entity - it only talks to this.
/// </summary>
public interface IUserDirectory
{
    Task<bool> ExistsAsync(string userId, CancellationToken ct);
    Task<IReadOnlyDictionary<string, UserSummary>> GetSummariesAsync(IEnumerable<string> userIds, CancellationToken ct);
    Task<IReadOnlyList<UserSummary>> SearchAsync(string query, int maxResults, CancellationToken ct);
}
