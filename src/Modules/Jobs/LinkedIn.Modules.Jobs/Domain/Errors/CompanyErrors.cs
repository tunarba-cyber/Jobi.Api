using LinkedIn.Shared.Abstractions.Primitives;

namespace LinkedIn.Modules.Jobs.Domain.Errors;

public static class CompanyErrors
{
    public static Error NotFound(long id) =>
        Error.NotFound("Company.NotFound", $"No company was found with id {id}.");

    public static Error NotFoundBySlug(string slug) =>
        Error.NotFound("Company.NotFound", $"No company was found with slug '{slug}'.");

    public static Error SlugAlreadyExists(string slug) =>
        Error.Conflict("Company.SlugExists", $"A company with slug '{slug}' already exists.");

    public static readonly Error AlreadyHasCompany =
        Error.Conflict("Company.AlreadyExists", "This account already has a company profile.");

    public static readonly Error NoCompanyYet =
        Error.NotFound("Company.NoneForUser", "This account has not created a company profile yet.");

    public static readonly Error NotOwner =
        Error.Forbidden("Company.NotOwner", "You do not own this company.");
}
