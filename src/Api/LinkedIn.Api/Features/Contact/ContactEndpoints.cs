using LinkedIn.Modules.Messaging.Domain.Entities;
using LinkedIn.Modules.Messaging.Infrastructure.Persistence;
using LinkedIn.Modules.Users.Infrastructure.Email;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Net;
using System.Net.Mail;

namespace LinkedIn.Api.Features.Contact;

public sealed record ContactRequest(string Name, string Email, string? Subject, string Message);

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
    ContactRequest request, IEmailSender emailSender, IConfiguration configuration,
    MessagingDbContext db, ILogger<Program> logger, CancellationToken ct)
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

        db.ContactMessages.Add(new ContactMessage
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Subject = request.Subject?.Trim(),
            Body = request.Message.Trim(),
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync(ct);

        var adminEmail = configuration["Contact:AdminEmail"];
        if (!string.IsNullOrWhiteSpace(adminEmail))
        {
            var safeName = WebUtility.HtmlEncode(request.Name.Trim());
            var safeEmail = WebUtility.HtmlEncode(request.Email.Trim());
            var safeMessage = WebUtility.HtmlEncode(request.Message.Trim()).Replace("\n", "<br>");
            var subject = string.IsNullOrWhiteSpace(request.Subject)
                ? $"New contact form message from {request.Name.Trim()}"
                : $"[Contact] {request.Subject.Trim()}";

            try
            {
                await emailSender.SendAsync(adminEmail, subject,
                    $"<p><strong>From:</strong> {safeName} ({safeEmail})</p><p>{safeMessage}</p>", ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Contact email notification failed; message is saved in the inbox.");
            }
        }

        return Results.Ok(new { message = "Your message has been sent. We'll get back to you soon." });
    }
}