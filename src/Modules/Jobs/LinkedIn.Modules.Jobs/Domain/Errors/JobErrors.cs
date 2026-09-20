using LinkedIn.Shared.Abstractions.Primitives;

namespace LinkedIn.Modules.Jobs.Domain.Errors;

/// <summary>
/// Every way a job operation can fail, declared in one place with stable codes
/// the frontend can switch on (instead of parsing English error strings).
/// </summary>
public static class JobErrors
{
    public static Error NotFound(long id) =>
        Error.NotFound("Job.NotFound", $"No job was found with id {id}.");

    public static Error NotFoundBySlug(string slug) =>
        Error.NotFound("Job.NotFound", $"No job was found with slug '{slug}'.");

    public static Error SlugAlreadyExists(string slug) =>
        Error.Conflict("Job.SlugExists", $"A job with slug '{slug}' already exists.");

    public static Error CategoryNotFound(long categoryId) =>
        Error.NotFound("Job.CategoryNotFound", $"No category was found with id {categoryId}.");
}
