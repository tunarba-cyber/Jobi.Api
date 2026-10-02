using System.Security.Claims;
using LinkedIn.Modules.Messaging.Features.GetConversations;
using LinkedIn.Modules.Messaging.Features.GetMessages;
using LinkedIn.Modules.Messaging.Features.MarkConversationRead;
using LinkedIn.Modules.Messaging.Features.SendMessage;
using LinkedIn.Shared.Abstractions.Primitives;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LinkedIn.Modules.Messaging.Features;

public sealed record SendMessageRequest(string RecipientId, string Body);

public static class MessagingEndpoints
{
    public static IEndpointRouteBuilder MapMessagingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/messaging")
            .WithTags("Messaging")
            .RequireAuthorization();

        group.MapPost("/messages", async (SendMessageRequest body, ClaimsPrincipal user, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new SendMessageCommand(user.GetUserId(), body.RecipientId, body.Body), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
        });

        group.MapGet("/conversations", async (ClaimsPrincipal user, ISender sender, CancellationToken ct,
            int page = 1, int pageSize = 20) =>
        {
            var result = await sender.Send(new GetConversationsQuery(user.GetUserId(), page, pageSize), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
        });

        group.MapGet("/conversations/{conversationId:guid}/messages", async (
            Guid conversationId, ClaimsPrincipal user, ISender sender, CancellationToken ct,
            DateTimeOffset? before = null, int pageSize = 30) =>
        {
            var result = await sender.Send(new GetMessagesQuery(user.GetUserId(), conversationId, before, pageSize), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error);
        });

        group.MapPost("/conversations/{conversationId:guid}/read", async (
            Guid conversationId, ClaimsPrincipal user, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new MarkConversationReadCommand(user.GetUserId(), conversationId), ct);
            return result.IsSuccess ? Results.NoContent() : ToProblem(result.Error);
        });

        return endpoints;
    }

    // Adapt to however your Auth endpoints map Error -> HTTP if you already have a helper.
    private static IResult ToProblem(Error error) =>
    Results.Problem(
        title: error.Code,
        detail: error.Description,
        statusCode: error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status400BadRequest
        });

    private static string GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? user.FindFirstValue("sub")
        ?? throw new InvalidOperationException("Authenticated user has no id claim.");
}
