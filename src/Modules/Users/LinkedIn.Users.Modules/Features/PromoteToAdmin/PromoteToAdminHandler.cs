using LinkedIn.Modules.Users.Domain.Entities;
using LinkedIn.Modules.Users.Domain.Enums;
using LinkedIn.Modules.Users.Domain.Errors;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LinkedIn.Modules.Users.Features.PromoteToAdmin;

internal sealed class PromoteToAdminHandler : IRequestHandler<PromoteToAdminCommand, Result>
{
    private readonly UserManager<AppUser> _userManager;

    public PromoteToAdminHandler(UserManager<AppUser> userManager) => _userManager = userManager;

    public async Task<Result> Handle(PromoteToAdminCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.TargetUserId);
        if (user is null)
            return Result.Failure(UserErrors.InvalidOrExpiredToken); // reusing an existing "not found" style error

        user.Role = UserRole.Admin;
        await _userManager.UpdateAsync(user);

        return Result.Success();
    }
}