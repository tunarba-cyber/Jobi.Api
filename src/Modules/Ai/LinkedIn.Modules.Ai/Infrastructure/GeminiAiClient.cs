using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LinkedIn.Modules.Ai.Domain;
using LinkedIn.Shared.Abstractions.Primitives;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinkedIn.Modules.Ai.Infrastructure;

internal sealed class GeminiAiClient : IAiClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly AiOptions _options;
    private readonly ILogger<GeminiAiClient> _logger;

    public GeminiAiClient(HttpClient http, IOptions<AiOptions> options, ILogger<GeminiAiClient> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.ApiKey);

    public async Task<Result<string>> CompleteAsync(string system, string userMessage, int maxTokens, CancellationToken ct)
    {
        if (!IsConfigured) return Result.Failure<string>(AiErrors.NotConfigured);

        var model = _options.Model.StartsWith("models/", StringComparison.Ordinal)
            ? _options.Model["models/".Length..]
            : _options.Model;

        using var request = new HttpRequestMessage(HttpMethod.Post, $"v1beta/models/{model}:generateContent");
        request.Headers.Add("x-goog-api-key", _options.ApiKey);
        request.Content = JsonContent.Create(new
        {
            systemInstruction = new { parts = new[] { new { text = system } } },
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = userMessage } } }
            },
            generationConfig = new { maxOutputTokens = maxTokens }
        }, options: Json);

        try
        {
            using var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Gemini API returned {Status}: {Body}",
                    (int)response.StatusCode, body.Length > 300 ? body[..300] : body);

                return Result.Failure<string>(
                    response.StatusCode is (HttpStatusCode)429 or HttpStatusCode.ServiceUnavailable
                        ? AiErrors.Busy
                        : AiErrors.Upstream);
            }

            var parsed = await response.Content.ReadFromJsonAsync<GeminiResponse>(Json, ct);

            if (!string.IsNullOrEmpty(parsed?.PromptFeedback?.BlockReason))
            {
                _logger.LogWarning("Gemini blocked the prompt: {Reason}", parsed.PromptFeedback.BlockReason);
                return Result.Failure<string>(AiErrors.Upstream);
            }

            var parts = parsed?.Candidates?.FirstOrDefault()?.Content?.Parts ?? new List<GeminiPart>();
            var text = string.Concat(parts.Select(p => p.Text ?? string.Empty)).Trim();

            if (text.Length == 0)
            {
                _logger.LogWarning("Gemini returned no text. FinishReason: {Reason}",
                    parsed?.Candidates?.FirstOrDefault()?.FinishReason);
                return Result.Failure<string>(AiErrors.Upstream);
            }

            return Result.Success(text);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException && !ct.IsCancellationRequested)
        {
            _logger.LogError(ex, "Gemini API call failed.");
            return Result.Failure<string>(AiErrors.Upstream);
        }
    }

    private sealed record GeminiResponse(List<GeminiCandidate>? Candidates, GeminiPromptFeedback? PromptFeedback);
    private sealed record GeminiCandidate(GeminiContent? Content, string? FinishReason);
    private sealed record GeminiContent(List<GeminiPart>? Parts);
    private sealed record GeminiPart(string? Text);
    private sealed record GeminiPromptFeedback(string? BlockReason);
}