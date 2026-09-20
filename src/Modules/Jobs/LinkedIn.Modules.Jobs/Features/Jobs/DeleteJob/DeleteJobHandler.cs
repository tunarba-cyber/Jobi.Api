using LinkedIn.Modules.Jobs.Domain.Errors;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Jobs.DeleteJob;

internal sealed class DeleteJobHandler : IRequestHandler<DeleteJobCommand, Result>
{
    private readonly JobsDbContext _db;

    public DeleteJobHandler(JobsDbContext db) => _db = db;

    public async Task<Result> Handle(DeleteJobCommand request, CancellationToken cancellationToken)
    {
        var job = await _db.Jobs
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);

        if (job is null)
            return Result.Failure(JobErrors.NotFound(request.Id));

        // SoftDeleteInterceptor turns this into an UPDATE - the row survives.
        _db.Jobs.Remove(job);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
