using LinkedIn.Modules.Jobs.Domain.Errors;
using LinkedIn.Modules.Jobs.Features.Categories.Dtos;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Categories.GetCategoryBySlug;

internal sealed class GetCategoryBySlugHandler
    : IRequestHandler<GetCategoryBySlugQuery, Result<CategoryDto>>
{
    private readonly JobsDbContext _db;

    public GetCategoryBySlugHandler(JobsDbContext db) => _db = db;

    public async Task<Result<CategoryDto>> Handle(
        GetCategoryBySlugQuery request,
        CancellationToken cancellationToken)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();

        var dto = await _db.Categories
            .AsNoTracking()
            .Where(c => c.Slug == slug)
            .Select(CategoryMappings.ToDto)
            .FirstOrDefaultAsync(cancellationToken);

        return dto is null
            ? Result.Failure<CategoryDto>(CategoryErrors.NotFoundBySlug(slug))
            : Result.Success(dto);
    }
}
