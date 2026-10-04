using LinkedIn.MVC.Models.Api;

namespace LinkedIn.MVC.Models.ViewModels;

public sealed class ConversationsViewModel
{
    public IReadOnlyList<ConversationDto> Items { get; init; } = Array.Empty<ConversationDto>();
}

public sealed class ConversationViewModel
{
    public long ConversationId { get; init; }
    public string OtherUserId { get; init; } = string.Empty;
    public IReadOnlyList<MessageDto> Messages { get; init; } = Array.Empty<MessageDto>();
}