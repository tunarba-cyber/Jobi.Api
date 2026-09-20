using LinkedIn.Shared.Abstractions.Primitives;

namespace LinkedIn.Modules.Jobs.Domain.Errors;

/// <summary>
/// Every way a category operation can fail, declared in one place with stable
/// codes the frontend can switch on (instead of parsing English error strings).
/// </summary>
public static class CategoryErrors
{
    public static Error NotFound(long id) =>
        Error.NotFound("Category.NotFound", $"No category was found with id {id}.");

    public static Error NotFoundBySlug(string slug) =>
        Error.NotFound("Category.NotFound", $"No category was found with slug '{slug}'.");

    public static Error SlugAlreadyExists(string slug) =>
        Error.Conflict("Category.SlugExists", $"A category with slug '{slug}' already exists.");

    public static Error NameAlreadyExists(string name) =>
        Error.Conflict("Category.NameExists", $"A category named '{name}' already exists.");
}
