using LinkedIn.Modules.Users.Infrastructure.Email;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LinkedIn.Api.Features.Contact;

public sealed record ContactRequest(string Name, string Email, string Subject, string Message);

internal static class ContactEndpoints
{
    public static void MapContactEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/contact", SendContactMessage)
            .WithTags("Contact")
            .WithName("SendContactMessage")
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> SendContactMessage(
        ContactRequest request,
        IEmailSender emailSender,
        IConfiguration configuration,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Message))
        {
            return Results.Problem(
                title: "Contact.InvalidRequest",
                detail: "Name, email, and message are all required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        // Where contact messages land - a real inbox, not stored in the DB at
        // all. This is a one-way notification, not a feature that needs a table.
        var adminEmail = configuration["Contact:AdminEmail"]
            ?? throw new InvalidOperationException("Contact:AdminEmail is not configured.");

        var subject = string.IsNullOrWhiteSpace(request.Subject)
            ? $"New contact form message from {request.Name}"
            : $"[Contact] {request.Subject}";

        var body = $"""
            <p><strong>From:</strong> {request.Name} ({request.Email})</p>
            <p><strong>Message:</strong></p>
            <p>{request.Message}</p>
            """;

        await emailSender.SendAsync(adminEmail, subject, body, ct);

        return Results.Ok(new { message = "Your message has been sent. We'll get back to you soon." });
    }
}