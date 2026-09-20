using FluentValidation;

namespace LinkedIn.Modules.Jobs.Features.Categories.UpdateCategory;

internal sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(c => c.Id).GreaterThan(0);

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .MaximumLength(100);

        RuleFor(c => c.Slug)
            .MaximumLength(120)
            .Matches("^[a-z0-9]+(-[a-z0-9]+)*$")
            .WithMessage("Slug may contain only lowercase letters, numbers and single hyphens.")
            .When(c => !string.IsNullOrWhiteSpace(c.Slug));

        RuleFor(c => c.IconUrl).MaximumLength(500);
        RuleFor(c => c.Description).MaximumLength(1000);
        RuleFor(c => c.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
