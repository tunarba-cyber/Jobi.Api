using FluentValidation;
using MediatR;

namespace LinkedIn.Shared.Infrastructure.Behaviors;

/// <summary>
/// Runs every registered FluentValidation validator before the handler executes.
///
/// This fixes a real bug in the Pronia template: it registered validators with
/// AddValidatorsFromAssembly but never wired them into the MediatR pipeline, so
/// CreateProductCommandValidator and friends never actually ran. Registering a
/// validator is not the same as invoking it.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = results
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
