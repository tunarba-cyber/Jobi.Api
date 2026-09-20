using LinkedIn.Modules.Users.Domain.Entities;
using LinkedIn.Modules.Users.Domain.Errors;
using LinkedIn.Modules.Users.Features.Dtos;
using LinkedIn.Modules.Users.Infrastructure.Persistence;
using LinkedIn.Modules.Users.Infrastructure.Tokens;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using RefreshTokenEntity = LinkedIn.Modules.Users.Domain.Entities.RefreshToken;

namespace LinkedIn.Modules.Users.Features.Login;

internal sealed class LoginHandler : IRequestHandler<LoginCommand, Result<AuthResultDto>>
{
    private readonly UserManager<AppUser> _userManager;
    private readonly UsersDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly TimeProvider _timeProvider;
    private readonly JwtOptions _jwtOptions;

    public LoginHandler(
        UserManager<AppUser> userManager,
        UsersDbContext db,
        ITokenService tokenService,
        TimeProvider timeProvider,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _db = db;
        _tokenService = tokenService;
        _timeProvider = timeProvider;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<Result<AuthResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Deliberately identical failure below for "no such user" and "wrong
        // password" - see UserErrors.InvalidCredentials.
        if (user is null)
            return Result.Failure<AuthResultDto>(UserErrors.InvalidCredentials);

        // Not using SignInManager, so lockout tracking has to be driven by hand:
        // check first, increment on failure, reset on success.
        if (await _userManager.IsLockedOutAsync(user))
            return Result.Failure<AuthResultDto>(UserErrors.AccountLockedOut);

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            await _userManager.AccessFailedAsync(user);
            return Result.Failure<AuthResultDto>(UserErrors.InvalidCredentials);
        }

        if (!await _userManager.IsEmailConfirmedAsync(user))
            return Result.Failure<AuthResultDto>(UserErrors.EmailNotConfirmed);

        await _userManager.ResetAccessFailedCountAsync(user);

        var now = _timeProvider.GetUtcNow();
        var accessToken = _tokenService.GenerateAccessToken(user);
        var rawRefreshToken = _tokenService.GenerateRefreshTokenValue();

        _db.RefreshTokens.Add(new RefreshTokenEntity
        {
            AppUserId = user.Id,
            TokenHash = _tokenService.Hash(rawRefreshToken),
            ExpiresAtUtc = now.AddDays(_jwtOptions.RefreshTokenDays)
        });
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new AuthResultDto(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            user.Role,
            accessToken,
            rawRefreshToken,
            now.AddMinutes(_jwtOptions.AccessTokenMinutes));

        return Result.Success(dto);
    }
}