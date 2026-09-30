using LinkedIn.Modules.Users.Infrastructure.Email;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Net;
using System.Net.Mail;

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
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 100 ||
            string.IsNullOrWhiteSpace(request.Email) || request.Email.Length > 255 ||
            !MailAddress.TryCreate(request.Email, out _) ||
            request.Subject?.Length > 200 ||
            string.IsNullOrWhiteSpace(request.Message) || request.Message.Length > 4000)
        {
            return Results.Problem(
                title: "Contact.InvalidRequest",
                detail: "Enter a valid name, email, subject, and message within the allowed lengths.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        // Where contact messages land - a real inbox, not stored in the DB at
        // all. This is a one-way notification, not a feature that needs a table.
        var adminEmail = configuration["Contact:AdminEmail"]
            ?? throw new InvalidOperationException("Contact:AdminEmail is not configured.");

        var safeName = WebUtility.HtmlEncode(request.Name.Trim());
        var safeEmail = WebUtility.HtmlEncode(request.Email.Trim());
        var safeMessage = WebUtility.HtmlEncode(request.Message.Trim())
            .Replace("\r\n", "<br>", StringComparison.Ordinal)
            .Replace("\n", "<br>", StringComparison.Ordinal);

        var subject = string.IsNullOrWhiteSpace(request.Subject)
            ? $"New contact form message from {request.Name.Trim()}"
            : $"[Contact] {request.Subject.Trim()}";

        var body = $"""
            <p><strong>From:</strong> {safeName} ({safeEmail})</p>
            <p><strong>Message:</strong></p>
            <p>{safeMessage}</p>
            """;

        await emailSender.SendAsync(adminEmail, subject, body, ct);

        return Results.Ok(new { message = "Your message has been sent. We'll get back to you soon." });
    }
}