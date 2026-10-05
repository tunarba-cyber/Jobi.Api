using LinkedIn.Modules.Users.Domain.Entities;
using LinkedIn.Modules.Users.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinkedIn.Modules.Users.Infrastructure;

/// <summary>
/// Runs once at startup. If no Admin exists yet, creates one from config -
/// the only way to get a first Admin account, since registration deliberately
/// blocks self-granting that role. Safe to run every startup: it's a no-op
/// once an Admin already exists.
/// </summary>
public static class AdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var anyAdminExists = userManager.Users.Any(u => u.Role == UserRole.Admin);
        if (anyAdminExists) return;

        var email = configuration["AdminSeed:Email"];
        var password = configuration["AdminSeed:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return; // not configured - nothing to seed, not an error

        var admin = new AppUser
        {
            UserName = email,
            Email = email,
            FirstName = "Admin",
            LastName = "User",
            Role = UserRole.Admin,
            EmailConfirmed = true, // seeded account - skip the confirmation flow entirely
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        await userManager.CreateAsync(admin, password);
    }
}