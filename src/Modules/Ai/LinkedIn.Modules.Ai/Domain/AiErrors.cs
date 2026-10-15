using LinkedIn.Shared.Abstractions.Primitives;

namespace LinkedIn.Modules.Ai.Domain;

public static class AiErrors
{
    public static readonly Error NotConfigured = Error.Failure("Ai.NotConfigured", "AI assistance isn't configured on this server.");
    public static readonly Error Busy = Error.Failure("Ai.Busy", "The AI service is busy. Please try again in a moment.");
    public static readonly Error Upstream = Error.Failure("Ai.Upstream", "The AI service couldn't complete the request.");
    public static readonly Error DailyLimit = Error.Failure("Ai.DailyLimit", "You've reached today's AI limit. Please try again tomorrow.");
}