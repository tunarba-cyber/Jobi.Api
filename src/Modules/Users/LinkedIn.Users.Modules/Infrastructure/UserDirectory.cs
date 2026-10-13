using LinkedIn.Modules.Users.Domain.Enums;
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
    public async Task<IReadOnlyList<UserSummary>> SearchAsync(string query, int maxResults, CancellationToken ct)
    {
        var q = query.Trim();
        if (q.Length < 2) return Array.Empty<UserSummary>();

        return await _db.Users.AsNoTracking()
            .Where(u => u.EmailConfirmed && (u.FirstName.Contains(q) || u.LastName.Contains(q)))
            .Take(maxResults)
            .Select(u => new UserSummary(u.Id, u.FirstName, u.LastName))
            .ToListAsync(ct);
    }
    public async Task<(IReadOnlyList<UserCard> Items, int Total)> BrowseAsync(string? query, string excludeUserId, int skip, int take, CancellationToken ct)
    {
        var q = query?.Trim();
        var source = _db.Users.AsNoTracking()
            .Where(u => u.EmailConfirmed && u.Role != UserRole.Admin && u.Id != excludeUserId)
            ;

        var total = await source.CountAsync(ct);
        var rows = await source.OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
            .Skip(skip).Take(take)
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.Role })
            .ToListAsync(ct);

        return (rows.Select(r => new UserCard(r.Id, r.FirstName, r.LastName, r.Role.ToString())).ToList(), total);
    }

    public async Task<IReadOnlyDictionary<string, UserCard>> GetCardsAsync(IEnumerable<string> ids, CancellationToken ct)
    {
        var list = ids.Distinct().ToList();
        var rows = await _db.Users.AsNoTracking()
            .Where(u => list.Contains(u.Id) && u.Role != UserRole.Admin)
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.Role })
            .ToListAsync(ct);
        return rows.ToDictionary(r => r.Id, r => new UserCard(r.Id, r.FirstName, r.LastName, r.Role.ToString()));
    }

    public Task<bool> IsFollowableAsync(string userId, CancellationToken ct) =>
        _db.Users.AnyAsync(u => u.Id == userId && u.EmailConfirmed && u.Role != UserRole.Admin, ct);
}
