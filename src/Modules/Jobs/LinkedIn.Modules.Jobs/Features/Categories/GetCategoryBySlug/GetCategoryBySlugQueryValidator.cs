using FluentValidation;

namespace LinkedIn.Modules.Jobs.Features.Categories.GetCategoryBySlug;

internal sealed class GetCategoryBySlugQueryValidator : AbstractValidator<GetCategoryBySlugQuery>
{
    public GetCategoryBySlugQueryValidator()
    {
        RuleFor(q => q.Slug)
            .NotEmpty()
            .MaximumLength(120);
    }
}
