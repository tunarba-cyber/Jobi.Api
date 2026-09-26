using LinkedIn.Shared.Abstractions.Primitives;

namespace LinkedIn.Modules.Jobs.Domain.Errors;

public static class SavedItemErrors
{
    public static Error AlreadySaved =>
        Error.Conflict("SavedItem.AlreadySaved", "This item is already saved.");

    public static Error NotFound =>
        Error.NotFound("SavedItem.NotFound", "This saved item was not found.");
}