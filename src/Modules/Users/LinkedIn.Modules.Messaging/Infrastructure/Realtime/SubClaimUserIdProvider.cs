using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace LinkedIn.Modules.Messaging.Infrastructure.Realtime;

// Maps SignalR's "user" concept to the JWT's NameIdentifier claim - same id
// used everywhere else in the app (ClaimTypes.NameIdentifier).
internal sealed class SubClaimUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) =>
        connection.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}