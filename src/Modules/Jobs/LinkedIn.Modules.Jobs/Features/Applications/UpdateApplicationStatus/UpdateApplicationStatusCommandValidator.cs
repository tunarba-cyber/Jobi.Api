using FluentValidation;

namespace LinkedIn.Modules.Jobs.Features.Applications.UpdateApplicationStatus;

internal sealed class UpdateApplicationStatusCommandValidator : AbstractValidator<UpdateApplicationStatusCommand>
{
    public UpdateApplicationStatusCommandValidator()
    {
        RuleFor(c => c.ApplicationId).GreaterThan(0);
        RuleFor(c => c.NewStatus).IsInEnum();
    }
}