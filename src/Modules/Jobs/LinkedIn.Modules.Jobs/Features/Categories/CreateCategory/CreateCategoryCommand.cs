using LinkedIn.Modules.Jobs.Features.Categories.Dtos;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Categories.CreateCategory;

/// <summary>Slug is optional - it is derived from Name when omitted.</summary>
public sealed record CreateCategoryCommand(
    string Name,
    string? Slug,
    string? IconUrl,
    string? Description,
    int DisplayOrder,
    bool IsFeatured) : IRequest<Result<CategoryDto>>;
