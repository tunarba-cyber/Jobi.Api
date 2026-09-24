using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Domain.Errors;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Applications.WithdrawApplication;

internal sealed class WithdrawApplicationHandler : IRequestHandler<WithdrawApplicationCommand, Result>
{
    private readonly JobsDbContext _db;

    public WithdrawApplicationHandler(JobsDbContext db) => _db = db;

    public async Task<Result> Handle(WithdrawApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await _db.Applications
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

        if (application is null)
            return Result.Failure(ApplicationErrors.NotFound(request.ApplicationId));

        if (application.CandidateUserId != request.RequestingUserId)
            return Result.Failure(ApplicationErrors.NotOwnedByCaller);

        if (application.Status is ApplicationStatus.Hired or ApplicationStatus.Rejected)
            return Result.Failure(ApplicationErrors.CannotWithdraw(
                $"This application is already {application.Status} and can no longer be withdrawn."));

        _db.Applications.Remove(application); // soft-deleted by SoftDeleteInterceptor
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}