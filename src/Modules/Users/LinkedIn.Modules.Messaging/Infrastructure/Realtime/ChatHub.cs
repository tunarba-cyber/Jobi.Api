using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LinkedIn.Modules.Messaging.Infrastructure.Realtime;

[Authorize]
public sealed class ChatHub : Hub
{
    // Clients just connect and listen for "ReceiveMessage" - no server methods needed yet.
}