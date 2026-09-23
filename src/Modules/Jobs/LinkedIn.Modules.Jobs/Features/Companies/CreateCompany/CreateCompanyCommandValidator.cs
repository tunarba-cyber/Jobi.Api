using FluentValidation;

namespace LinkedIn.Modules.Jobs.Features.Companies.CreateCompany;

internal sealed class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(150);

        RuleFor(c => c.Slug)
            .MaximumLength(180)
            .Matches("^[a-z0-9]+(-[a-z0-9]+)*$")
            .WithMessage("Slug may contain only lowercase letters, numbers and single hyphens.")
            .When(c => !string.IsNullOrWhiteSpace(c.Slug));

        RuleFor(c => c.Description).MaximumLength(2000);

        RuleFor(c => c.WebsiteUrl)
            .MaximumLength(500)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Website URL must be a valid absolute URL.")
            .When(c => !string.IsNullOrWhiteSpace(c.WebsiteUrl));

        RuleFor(c => c.LogoUrl).MaximumLength(500);
    }
}
