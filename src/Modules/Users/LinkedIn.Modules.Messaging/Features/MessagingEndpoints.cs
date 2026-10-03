using System.Security.Claims;
using LinkedIn.Modules.Messaging.Features.Dtos;
using LinkedIn.Modules.Messaging.Features.GetConversations;
using LinkedIn.Modules.Messaging.Features.GetMessages;
using LinkedIn.Modules.Messaging.Features.MarkConversationRead;
using LinkedIn.Modules.Messaging.Features.SendMessage;
using LinkedIn.Shared.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LinkedIn.Modules.Messaging.Features;

internal static class MessagingEndpoints
{
    public static void MapMessagingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/messages").WithTags("Messaging").RequireAuthorization();

        group.MapGet("/", GetConversations).WithName("GetMyConversations");
        group.MapGet("/{conversationId:long}", GetMessages).WithName("GetConversationMessages");
        group.MapPost("/", Send).WithName("SendMessage");
    }

    private static async Task<IResult> GetConversations(ClaimsPrincipal user, ISender sender, CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await sender.Send(new GetConversationsQuery(userId), ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetMessages(long conversationId, ClaimsPrincipal user, ISender sender, CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;

        await sender.Send(new MarkConversationReadCommand(conversationId, userId), ct);
        var result = await sender.Send(new GetMessagesQuery(conversationId, userId), ct);
        return result.ToHttpResult();
    }

    private static async Task<IResult> Send(SendMessageRequest request, ClaimsPrincipal user, ISender sender, CancellationToken ct)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await sender.Send(new SendMessageCommand(userId, request.RecipientUserId, request.Content), ct);
        return result.ToHttpResult();
    }

    private sealed record SendMessageRequest(string RecipientUserId, string Content);
}