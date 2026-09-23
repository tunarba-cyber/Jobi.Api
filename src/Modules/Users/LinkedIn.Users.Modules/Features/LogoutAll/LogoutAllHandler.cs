using LinkedIn.Modules.Users.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Users.Features.LogoutAll;

internal sealed class LogoutAllHandler : IRequestHandler<LogoutAllCommand, Result>
{
    private readonly UsersDbContext _db;
    private readonly TimeProvider _timeProvider;

    public LogoutAllHandler(UsersDbContext db, TimeProvider timeProvider)
    {
        _db = db;
        _timeProvider = timeProvider;
    }

    public async Task<Result> Handle(LogoutAllCommand request, CancellationToken cancellationToken)
    {
        var activeTokens = await _db.RefreshTokens
            .Where(t => t.AppUserId == request.UserId && t.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        var now = _timeProvider.GetUtcNow();
        foreach (var token in activeTokens)
            token.RevokedAtUtc = now;

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
