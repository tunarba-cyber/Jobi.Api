using Microsoft.Extensions.Options;

namespace LinkedIn.Modules.Ai.Infrastructure;

/// <summary>In-memory per-user daily cap. Resets at UTC midnight and on app restart.</summary>
internal sealed class DailyQuota
{
    private readonly Dictionary<string, (DateOnly Day, int Count)> _used = new();
    private readonly object _gate = new();
    private readonly int _limit;

    public DailyQuota(IOptions<AiOptions> options) => _limit = options.Value.DailyLimitPerUser;

    public bool TryConsume(string userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        lock (_gate)
        {
            var count = _used.TryGetValue(userId, out var cur) && cur.Day == today ? cur.Count : 0;
            if (count >= _limit) return false;
            _used[userId] = (today, count + 1);
            return true;
        }
    }
}