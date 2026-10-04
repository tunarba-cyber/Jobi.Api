using LinkedIn.MVC.Models.Api;

namespace LinkedIn.MVC.Models.ViewModels;

public sealed class ConversationsViewModel
{
    public string OtherUserName { get; init; } = string.Empty;
    public IReadOnlyList<ConversationDto> Items { get; init; } = Array.Empty<ConversationDto>();
}

public sealed class ConversationViewModel
{
    public string OtherUserName { get; init; } = string.Empty;
    public long ConversationId { get; init; }
    public string OtherUserId { get; init; } = string.Empty;
    public IReadOnlyList<MessageDto> Messages { get; init; } = Array.Empty<MessageDto>();
}