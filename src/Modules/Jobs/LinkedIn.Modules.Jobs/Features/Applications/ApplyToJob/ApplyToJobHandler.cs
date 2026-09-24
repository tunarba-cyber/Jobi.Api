using LinkedIn.Modules.Jobs.Domain.Entities;
using LinkedIn.Modules.Jobs.Domain.Enums;
using LinkedIn.Modules.Jobs.Domain.Errors;
using LinkedIn.Modules.Jobs.Features.Applications.Dtos;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Applications.ApplyToJob;

internal sealed class ApplyToJobHandler : IRequestHandler<ApplyToJobCommand, Result<ApplicationDto>>
{
    private readonly JobsDbContext _db;

    public ApplyToJobHandler(JobsDbContext db) => _db = db;

    public async Task<Result<ApplicationDto>> Handle(ApplyToJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == request.JobId, cancellationToken);
        if (job is null)
            return Result.Failure<ApplicationDto>(JobErrors.NotFound(request.JobId));

        if (job.Status != JobStatus.Active)
            return Result.Failure<ApplicationDto>(ApplicationErrors.JobNotOpen);

        var alreadyApplied = await _db.Applications
            .AnyAsync(a => a.JobId == request.JobId && a.CandidateUserId == request.CandidateUserId, cancellationToken);
        if (alreadyApplied)
            return Result.Failure<ApplicationDto>(ApplicationErrors.AlreadyApplied);

        var application = new Application
        {
            JobId = request.JobId,
            CandidateUserId = request.CandidateUserId,
            CandidateName = request.CandidateName,
            CandidateEmail = request.CandidateEmail,
            ResumeUrl = request.ResumeUrl?.Trim(),
            CoverLetter = request.CoverLetter?.Trim()
        };

        _db.Applications.Add(application);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = await _db.Applications
            .AsNoTracking()
            .Where(a => a.Id == application.Id)
            .Select(ApplicationMappings.ToDto)
            .FirstAsync(cancellationToken);

        return Result.Success(dto);
    }
}