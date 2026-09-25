using LinkedIn.Shared.Abstractions.Primitives;

namespace LinkedIn.Modules.Jobs.Domain.Errors;

public static class CandidateErrors
{
    public static Error NotFound(long id) =>
        Error.NotFound("Candidate.NotFound", $"No candidate profile was found with id {id}.");

    public static Error NotFoundBySlug(string slug) =>
        Error.NotFound("Candidate.NotFound", $"No candidate profile was found with slug '{slug}'.");

    public static Error NoProfileYet =>
        Error.NotFound("Candidate.NoProfileYet", "You have not created a candidate profile yet.");

    public static Error SlugAlreadyExists(string slug) =>
        Error.Conflict("Candidate.SlugExists", $"A candidate profile with slug '{slug}' already exists.");
}