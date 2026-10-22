using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Shared.Infrastructure.Http;

public sealed record MappedError(int Status, string Title, string Detail);

public static class ExceptionMapper
{
    /// <summary>Exceptions that are a normal outcome in practice. Null means "unexpected": answer with a generic 500.</summary>
    public static MappedError? Map(Exception ex) => ex switch
    {
        BadHttpRequestException b => new(b.StatusCode, "Request.Invalid", "The request was malformed or incomplete."),
        KeyNotFoundException k => new(404, "NotFound", k.Message),
        DbUpdateConcurrencyException => new(409, "Conflict.Concurrent", "This item was changed by someone else. Reload and try again."),
        DbUpdateException d when IsUniqueViolation(d) => new(409, "Conflict.Duplicate", "That already exists."),
        TimeoutException => new(504, "Timeout", "The operation took too long. Please try again."),
        _ => null
    };

    // SQL Server error 2601 / 2627 = unique index or key violated. Read by name so this project needs no SqlClient package.
    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is { } inner
        && inner.GetType().Name == "SqlException"
        && inner.GetType().GetProperty("Number")?.GetValue(inner) is int n
        && n is 2601 or 2627;
}