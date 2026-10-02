using LinkedIn.Modules.Users.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Contracts;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Users.Infrastructure;

internal sealed class UserDirectory : IUserDirectory
{
    private readonly UsersDbContext _db;
    public UserDirectory(UsersDbContext db) => _db = db;

    public Task<bool> ExistsAsync(string userId, CancellationToken ct) =>
        _db.Users.AnyAsync(u => u.Id == userId && u.EmailConfirmed, ct);

    public async Task<IReadOnlyDictionary<string, UserSummary>> GetSummariesAsync(
        IEnumerable<string> userIds, CancellationToken ct)
    {
        var ids = userIds.Distinct().ToList();
        return await _db.Users.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => new UserSummary(u.Id, u.FirstName, u.LastName), ct);
    }
}
