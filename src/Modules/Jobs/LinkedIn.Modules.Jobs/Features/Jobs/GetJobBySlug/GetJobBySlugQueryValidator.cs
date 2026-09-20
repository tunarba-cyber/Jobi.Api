using FluentValidation;

namespace LinkedIn.Modules.Jobs.Features.Jobs.GetJobBySlug;

internal sealed class GetJobBySlugQueryValidator : AbstractValidator<GetJobBySlugQuery>
{
    public GetJobBySlugQueryValidator()
    {
        RuleFor(q => q.Slug)
            .NotEmpty()
            .MaximumLength(180);
    }
}
