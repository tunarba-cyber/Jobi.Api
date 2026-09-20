using LinkedIn.Modules.Jobs.Domain.Errors;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Categories.DeleteCategory;

internal sealed class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly JobsDbContext _db;

    public DeleteCategoryHandler(JobsDbContext db) => _db = db;

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
            return Result.Failure(CategoryErrors.NotFound(request.Id));

        // SoftDeleteInterceptor turns this into an UPDATE - the row survives.
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
