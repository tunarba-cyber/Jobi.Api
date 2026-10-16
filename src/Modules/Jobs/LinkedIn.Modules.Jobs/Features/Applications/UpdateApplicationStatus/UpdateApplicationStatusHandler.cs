using LinkedIn.Modules.Jobs.Domain.Errors;
using LinkedIn.Modules.Jobs.Features.Applications.Dtos;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using A=LinkedIn.Shared.Abstractions.Contracts;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Applications.UpdateApplicationStatus;

internal sealed class UpdateApplicationStatusHandler : IRequestHandler<UpdateApplicationStatusCommand, Result<ApplicationDto>>
{
    private readonly JobsDbContext _db;
    private readonly A.INotificationPublisher _notify;

    public UpdateApplicationStatusHandler(JobsDbContext db, A.INotificationPublisher notify)
    {
        _db = db;
        _notify = notify;
    }

    public async Task<Result<ApplicationDto>> Handle(UpdateApplicationStatusCommand request, CancellationToken cancellationToken)
    {
        var application = await _db.Applications
            .Include(a => a.Job)
            .ThenInclude(j => j!.Company)
            .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, cancellationToken);

        if (application is null)
            return Result.Failure<ApplicationDto>(ApplicationErrors.NotFound(request.ApplicationId));

        if (application.Job?.Company?.OwnerUserId != request.RequestingUserId)
            return Result.Failure<ApplicationDto>(ApplicationErrors.JobNotOwnedByCaller);

        var previous = application.Status;
        application.Status = request.NewStatus;
        await _db.SaveChangesAsync(cancellationToken);

        if (previous != request.NewStatus)
            await _notify.PublishAsync(application.CandidateUserId, A.NotificationTypes.ApplicationStatus, "Application update",
                $"{application.Job!.Title}: now {request.NewStatus}.", "/Application/My", ct: cancellationToken);

        var dto = await _db.Applications
            .AsNoTracking()
            .Where(a => a.Id == application.Id)
            .Select(ApplicationMappings.ToDto)
            .FirstAsync(cancellationToken);

        return Result.Success(dto);
    }
}