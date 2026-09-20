using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;

namespace LinkedIn.Modules.Jobs.Features.Categories.DeleteCategory;

public sealed record DeleteCategoryCommand(long Id) : IRequest<Result>;
