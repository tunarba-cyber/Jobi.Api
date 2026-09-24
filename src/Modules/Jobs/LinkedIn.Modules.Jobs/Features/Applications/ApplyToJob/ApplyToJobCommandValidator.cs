using FluentValidation;

namespace LinkedIn.Modules.Jobs.Features.Applications.ApplyToJob;

internal sealed class ApplyToJobCommandValidator : AbstractValidator<ApplyToJobCommand>
{
    public ApplyToJobCommandValidator()
    {
        RuleFor(c => c.JobId).GreaterThan(0);
        RuleFor(c => c.ResumeUrl).MaximumLength(500);
        RuleFor(c => c.CoverLetter).MaximumLength(4000);
    }
}