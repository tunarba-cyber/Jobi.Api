using FluentValidation;

namespace LinkedIn.Modules.Jobs.Features.Companies.UpdateMyCompany;

internal sealed class UpdateMyCompanyCommandValidator : AbstractValidator<UpdateMyCompanyCommand>
{
    public UpdateMyCompanyCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(200);

        RuleFor(c => c.Description).MaximumLength(2000);
        RuleFor(c => c.WebsiteUrl).MaximumLength(500);
        RuleFor(c => c.LogoUrl).MaximumLength(500);
    }
}