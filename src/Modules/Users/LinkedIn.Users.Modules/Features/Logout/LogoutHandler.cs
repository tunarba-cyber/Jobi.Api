using LinkedIn.Modules.Users.Infrastructure.Persistence;
using LinkedIn.Modules.Users.Infrastructure.Tokens;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Users.Features.Logout;

internal sealed class LogoutHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly UsersDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly TimeProvider _timeProvider;

    public LogoutHandler(UsersDbContext db, ITokenService tokenService, TimeProvider timeProvider)
    {
        _db = db;
        _tokenService = tokenService;
        _timeProvider = timeProvider;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var hash = _tokenService.Hash(request.RefreshToken);

        var storedToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        // Logging out an already-invalid/unknown token is still a "success" from
        // the caller's point of view - there is nothing left to revoke either way.
        if (storedToken is not null && storedToken.RevokedAtUtc is null)
        {
            storedToken.RevokedAtUtc = _timeProvider.GetUtcNow();
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
