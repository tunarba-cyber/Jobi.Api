namespace LinkedIn.Shared.Abstractions.Contracts;

/// <summary>Sends an email to a user by id. Implemented by the Users module, which owns addresses.</summary>
public interface IUserMailer
{
    Task SendAsync(string userId, string subject, string htmlBody, CancellationToken ct);
}