using FluentValidation;
using LinkedIn.Shared.Infrastructure.Http;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LinkedIn.Api.Middleware;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    { _logger = logger; _environment = environment; }

    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        // The caller closed the connection. Nothing to send, and not an error.
        if (ex is OperationCanceledException && ctx.RequestAborted.IsCancellationRequested)
        {
            ctx.Response.StatusCode = 499;
            return true;
        }

        var path = ctx.Request.Path;
        if (ctx.Response.HasStarted)
        {
            _logger.LogError(ex, "Exception after the response had started for {Path}", path);
            return false;
        }

        ProblemDetails problem;
        if (ex is ValidationException ve)
        {
            _logger.LogInformation("Validation failed for {Method} {Path}", ctx.Request.Method, path);
            problem = new ValidationProblemDetails(
                ve.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            };
        }
        else if (ExceptionMapper.Map(ex) is { } mapped)
        {
            _logger.LogInformation("{Title} on {Method} {Path}: {Message}", mapped.Title, ctx.Request.Method, path, ex.Message);
            problem = new ProblemDetails { Status = mapped.Status, Title = mapped.Title, Detail = mapped.Detail };
        }
        else
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}", ctx.Request.Method, path);
            problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = _environment.IsDevelopment() ? ex.ToString() : null   // never leak internals in production
            };
        }

        problem.Instance = path;
        problem.Extensions["traceId"] = ctx.TraceIdentifier;
        ctx.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;

        // The runtime type matters: written as the base type, a validation problem loses its "errors".
        await ctx.Response.WriteAsJsonAsync((object)problem, problem.GetType(), cancellationToken: ct);
        return true;
    }
}