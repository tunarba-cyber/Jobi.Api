using FluentValidation;

namespace LinkedIn.Modules.Jobs.Features.Jobs.UpdateJob;

internal sealed class UpdateJobCommandValidator : AbstractValidator<UpdateJobCommand>
{
    public UpdateJobCommandValidator()
    {
        RuleFor(c => c.Id).GreaterThan(0);

        RuleFor(c => c.Title)
            .NotEmpty().WithMessage("Job title is required.")
            .MaximumLength(150);

        RuleFor(c => c.Slug)
            .MaximumLength(180)
            .Matches("^[a-z0-9]+(-[a-z0-9]+)*$")
            .WithMessage("Slug may contain only lowercase letters, numbers and single hyphens.")
            .When(c => !string.IsNullOrWhiteSpace(c.Slug));

        RuleFor(c => c.Description)
            .NotEmpty().WithMessage("Job description is required.")
            .MaximumLength(4000);

        RuleFor(c => c.CategoryId).GreaterThan(0);

        RuleFor(c => c.CompanyName)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(150);

        RuleFor(c => c.Location)
            .NotEmpty().WithMessage("Location is required.")
            .MaximumLength(150);

        RuleFor(c => c.JobType).IsInEnum();
        RuleFor(c => c.ExperienceLevel).IsInEnum();
        RuleFor(c => c.Status).IsInEnum();

        RuleFor(c => c.SalaryMin).GreaterThanOrEqualTo(0).When(c => c.SalaryMin.HasValue);
        RuleFor(c => c.SalaryMax).GreaterThanOrEqualTo(0).When(c => c.SalaryMax.HasValue);

        RuleFor(c => c)
            .Must(c => !c.SalaryMin.HasValue || !c.SalaryMax.HasValue || c.SalaryMin <= c.SalaryMax)
            .WithMessage("SalaryMin cannot be greater than SalaryMax.")
            .WithName("SalaryMin");

        RuleFor(c => c.VacancyCount).GreaterThan(0);
    }
}
