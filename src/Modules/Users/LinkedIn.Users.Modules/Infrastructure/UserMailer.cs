using LinkedIn.Modules.Users.Infrastructure.Email;
using LinkedIn.Modules.Users.Infrastructure.Persistence;
using LinkedIn.Shared.Abstractions.Contracts;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Users.Infrastructure;

internal sealed class UserMailer : IUserMailer
{
    private readonly UsersDbContext _db;
    private readonly IEmailSender _sender;
    public UserMailer(UsersDbContext db, IEmailSender sender) { _db = db; _sender = sender; }

    public async Task SendAsync(string userId, string subject, string htmlBody, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var email = await _db.Users.AsNoTracking()
            .Where(u => u.Id == userId && u.EmailConfirmed && (u.LockoutEnd == null || u.LockoutEnd <= now))
            .Select(u => u.Email).FirstOrDefaultAsync(ct);

        if (!string.IsNullOrWhiteSpace(email))
            await _sender.SendAsync(email, subject, htmlBody, ct);
    }
}