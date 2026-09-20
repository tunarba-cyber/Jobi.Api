using LinkedIn.Shared.Abstractions.Primitives;
using Microsoft.AspNetCore.Http;

namespace LinkedIn.Shared.Infrastructure.Http;

/// <summary>
/// One place that decides how a domain Result becomes an HTTP response.
/// Endpoints stay one-liners and status codes stay consistent across modules.
/// </summary>
public static class ResultExtensions
{
    public static IResult ToHttpResult(this Result result) =>
        result.IsSuccess ? Results.NoContent() : Problem(result.Error);

    public static IResult ToHttpResult<TValue>(this Result<TValue> result) =>
        result.IsSuccess ? Results.Ok(result.Value) : Problem(result.Error);

    public static IResult ToCreatedResult<TValue>(this Result<TValue> result, Func<TValue, string> locationFactory) =>
        result.IsSuccess
            ? Results.Created(locationFactory(result.Value), result.Value)
            : Problem(result.Error);

    private static IResult Problem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(
            title: error.Code,
            detail: error.Description,
            statusCode: statusCode);
    }
}
