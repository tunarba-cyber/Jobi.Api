using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LinkedIn.Modules.Messaging.Infrastructure.Realtime;

/// <summary>
/// Push-only hub. Clients SEND through the REST endpoint (so validation, auth and
/// persistence live in one place - the MediatR pipeline) and only RECEIVE here:
///   "MessageReceived"  (MessageDto)
///   "MessagesRead"     (conversationId, readerId)
/// </summary>
[Authorize]
public sealed class ChatHub : Hub;
