using LinkedIn.Modules.Users.Domain.Entities;
using LinkedIn.Shared.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LinkedIn.Modules.Users.Infrastructure.Persistence;

/// <summary>
/// IdentityUserContext (not the full IdentityDbContext) - this app uses a plain
/// UserRole enum instead of ASP.NET Identity's Role/UserRole/RoleClaim tables,
/// so there is no reason to create tables for a role system that is never used.
/// </summary>
public sealed class UsersDbContext : IdentityUserContext<AppUser>
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Must run first - this is what actually configures the AspNetUsers /
        // AspNetUserClaims / AspNetUserLogins / AspNetUserTokens tables.
        base.OnModelCreating(builder);

        builder.HasDefaultSchema(UsersSchema.Name);

        builder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);

        // Harmless no-op for the Identity tables (they don't implement
        // ISoftDeletable) - only affects entities in this module that opt in.
        builder.ApplySoftDeleteQueryFilter();
    }
}
