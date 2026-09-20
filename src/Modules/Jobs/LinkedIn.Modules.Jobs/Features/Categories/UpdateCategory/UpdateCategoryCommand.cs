using LinkedIn.Modules.Jobs.Features.Categories.Dtos;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Categories.UpdateCategory;

public sealed record UpdateCategoryCommand(
    long Id,
    string Name,
    string? Slug,
    string? IconUrl,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    bool IsFeatured) : IRequest<Result<CategoryDto>>;
