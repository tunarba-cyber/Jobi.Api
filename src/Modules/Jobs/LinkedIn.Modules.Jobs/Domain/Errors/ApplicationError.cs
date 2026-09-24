using LinkedIn.Shared.Abstractions.Primitives;

namespace LinkedIn.Modules.Jobs.Domain.Errors;

public static class ApplicationErrors
{
    public static Error NotFound(long id) =>
        Error.NotFound("Application.NotFound", $"No application was found with id {id}.");

    public static Error JobNotOpen =>
        Error.Conflict("Application.JobNotOpen", "This job is not currently accepting applications.");

    public static Error AlreadyApplied =>
        Error.Conflict("Application.AlreadyApplied", "You have already applied to this job.");

    public static Error NotOwnedByCaller =>
        Error.Forbidden("Application.Forbidden", "You do not have access to this application.");

    public static Error JobNotOwnedByCaller =>
        Error.Forbidden("Application.JobForbidden", "You do not own the company that posted this job.");

    public static Error CannotWithdraw(string reason) =>
        Error.Conflict("Application.CannotWithdraw", reason);
}