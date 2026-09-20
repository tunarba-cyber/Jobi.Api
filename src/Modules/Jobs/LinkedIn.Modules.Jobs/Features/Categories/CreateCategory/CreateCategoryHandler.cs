using LinkedIn.Modules.Jobs.Domain.Entities;
using LinkedIn.Modules.Jobs.Domain.Errors;
using LinkedIn.Modules.Jobs.Features.Categories.Dtos;
using LinkedIn.Modules.Jobs.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Jobs.Features.Categories.CreateCategory;

internal sealed class CreateCategoryHandler
    : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    private readonly JobsDbContext _db;

    public CreateCategoryHandler(JobsDbContext db) => _db = db;

    public async Task<Result<CategoryDto>> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugGenerator.Generate(request.Name)
            : request.Slug.Trim().ToLowerInvariant();

        var name = request.Name.Trim();

        if (await _db.Categories.AnyAsync(c => c.Slug == slug, cancellationToken))
            return Result.Failure<CategoryDto>(CategoryErrors.SlugAlreadyExists(slug));

        if (await _db.Categories.AnyAsync(c => c.Name == name, cancellationToken))
            return Result.Failure<CategoryDto>(CategoryErrors.NameAlreadyExists(name));

        var category = new Category
        {
            Name = name,
            Slug = slug,
            IconUrl = request.IconUrl?.Trim(),
            Description = request.Description?.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsFeatured = request.IsFeatured,
            IsActive = true
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(category.ToCategoryDto());
    }
}
