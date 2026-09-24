using LinkedIn.Modules.Jobs.Domain.Errors;
using LinkedIn.Modules.Jobs.Features.Applications.Dtos;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Applications.UpdateApplicationStatus;

internal sealed class UpdateApplicationStatusHandler : IRequestHandler<UpdateApplicationStatusCommand, Result<ApplicationDto>>
{
    private readonly JobsDbContext _db;

    public UpdateApplicationStatusHandler(JobsDbContext db) => _db = db;

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

        application.Status = request.NewStatus;
        await _db.SaveChangesAsync(cancellationToken);

        var dto = await _db.Applications
            .AsNoTracking()
            .Where(a => a.Id == application.Id)
            .Select(ApplicationMappings.ToDto)
            .FirstAsync(cancellationToken);

        return Result.Success(dto);
    }
}