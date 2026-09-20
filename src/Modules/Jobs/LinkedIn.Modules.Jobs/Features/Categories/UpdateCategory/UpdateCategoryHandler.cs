using LinkedIn.Modules.Jobs.Domain.Errors;
using LinkedIn.Modules.Jobs.Features.Categories.Dtos;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Categories.UpdateCategory;

internal sealed class UpdateCategoryHandler
    : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    private readonly JobsDbContext _db;

    public UpdateCategoryHandler(JobsDbContext db) => _db = db;

    public async Task<Result<CategoryDto>> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _db.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
            return Result.Failure<CategoryDto>(CategoryErrors.NotFound(request.Id));

        var name = request.Name.Trim();
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugGenerator.Generate(name)
            : request.Slug.Trim().ToLowerInvariant();

        // Uniqueness checks must exclude the row being edited.
        if (await _db.Categories.AnyAsync(c => c.Slug == slug && c.Id != request.Id, cancellationToken))
            return Result.Failure<CategoryDto>(CategoryErrors.SlugAlreadyExists(slug));

        if (await _db.Categories.AnyAsync(c => c.Name == name && c.Id != request.Id, cancellationToken))
            return Result.Failure<CategoryDto>(CategoryErrors.NameAlreadyExists(name));

        category.Name = name;
        category.Slug = slug;
        category.IconUrl = request.IconUrl?.Trim();
        category.Description = request.Description?.Trim();
        category.DisplayOrder = request.DisplayOrder;
        category.IsActive = request.IsActive;
        category.IsFeatured = request.IsFeatured;

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(category.ToCategoryDto());
    }
}
