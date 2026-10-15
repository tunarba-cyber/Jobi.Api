using LinkedIn.Shared.Abstractions.Primitives;

namespace LinkedIn.Modules.Ai.Infrastructure;

public interface IAiClient
{
    bool IsConfigured { get; }
    Task<Result<string>> CompleteAsync(string system, string userMessage, int maxTokens, CancellationToken ct);
}