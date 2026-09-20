using LinkedIn.Modules.Users.Domain.Entities;
using LinkedIn.Modules.Users.Domain.Errors;
using LinkedIn.Modules.Users.Features.Dtos;
using LinkedIn.Modules.Users.Infrastructure.Persistence;
using LinkedIn.Modules.Users.Infrastructure.Tokens;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinkedIn.Modules.Users.Features.RefreshToken;

internal sealed class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResultDto>>
{
    private readonly UsersDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly TimeProvider _timeProvider;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<RefreshTokenHandler> _logger;

    public RefreshTokenHandler(
        UsersDbContext db,
        UserManager<AppUser> userManager,
        ITokenService tokenService,
        TimeProvider timeProvider,
        IOptions<JwtOptions> jwtOptions,
        ILogger<RefreshTokenHandler> logger)
    {
        _db = db;
        _userManager = userManager;
        _tokenService = tokenService;
        _timeProvider = timeProvider;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<Result<AuthResultDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var incomingHash = _tokenService.Hash(request.RefreshToken);

        var storedToken = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == incomingHash, cancellationToken);

        if (storedToken is null)
            return Result.Failure<AuthResultDto>(UserErrors.InvalidOrExpiredToken);

        if (storedToken.RevokedAtUtc is not null)
        {
            // This token was already rotated once. Someone presenting it again
            // means either a bug on the client, or a leaked token being replayed
            // after the legitimate user already refreshed - worth knowing about
            // even though the response to the caller stays the same generic error.
            _logger.LogWarning(
                "Reuse of an already-revoked refresh token detected for user {UserId}.",
                storedToken.AppUserId);
            return Result.Failure<AuthResultDto>(UserErrors.InvalidOrExpiredToken);
        }

        if (!storedToken.IsActive)
            return Result.Failure<AuthResultDto>(UserErrors.InvalidOrExpiredToken);

        var user = await _userManager.FindByIdAsync(storedToken.AppUserId);
        if (user is null)
            return Result.Failure<AuthResultDto>(UserErrors.InvalidOrExpiredToken);

        var now = _timeProvider.GetUtcNow();
        var newRawRefreshToken = _tokenService.GenerateRefreshTokenValue();
        var newHash = _tokenService.Hash(newRawRefreshToken);

        // Rotate: the old row is revoked and points at what replaced it, rather
        // than being deleted - keeps a full audit trail of the token chain.
        storedToken.RevokedAtUtc = now;
        storedToken.ReplacedByTokenHash = newHash;

        _db.RefreshTokens.Add(new RefreshToken
        {
            AppUserId = user.Id,
            TokenHash = newHash,
            ExpiresAtUtc = now.AddDays(_jwtOptions.RefreshTokenDays)
        });

        await _db.SaveChangesAsync(cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user);

        var dto = new AuthResultDto(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.Role,
            accessToken,
            newRawRefreshToken,
            now.AddMinutes(_jwtOptions.AccessTokenMinutes));

        return Result.Success(dto);
    }
}
