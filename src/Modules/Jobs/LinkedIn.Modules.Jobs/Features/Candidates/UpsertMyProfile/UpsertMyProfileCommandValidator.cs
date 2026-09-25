using FluentValidation;

namespace LinkedIn.Modules.Jobs.Features.Candidates.UpsertMyProfile;

internal sealed class UpsertMyProfileCommandValidator : AbstractValidator<UpsertMyProfileCommand>
{
    public UpsertMyProfileCommandValidator()
    {
        RuleFor(c => c.FullName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Headline).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Bio).MaximumLength(2000);
        RuleFor(c => c.Location).MaximumLength(200);
        RuleFor(c => c.PhotoUrl).MaximumLength(500);
        RuleFor(c => c.ResumeUrl).MaximumLength(500);
        RuleFor(c => c.Skills).MaximumLength(1000);
        RuleFor(c => c.ExperienceLevel).IsInEnum();
    }
}