using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace LinkedIn.Modules.Messaging.Infrastructure.Realtime;

/// <summary>Makes Clients.User(id) target the same id the REST endpoints see.</summary>
internal sealed class SubClaimUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) =>
        connection.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? connection.User.FindFirst("sub")?.Value;
}
