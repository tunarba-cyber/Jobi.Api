using LinkedIn.Modules.Users.Domain.Entities;
using LinkedIn.Modules.Users.Features;
using LinkedIn.Modules.Users.Infrastructure.Email;
using LinkedIn.Modules.Users.Infrastructure.Persistence;
using LinkedIn.Modules.Users.Infrastructure.Tokens;
using LinkedIn.Shared.Abstractions.Modules;
using LinkedIn.Shared.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinkedIn.Modules.Users;

/// <summary>
/// Self-registering module - same contract as JobsModule. This is what makes
/// a "half-registered module" (referenced but never wired up, which is exactly
/// what happened to Users in the old Pronia codebase) structurally impossible:
/// referencing this assembly from the host is the only step required.
/// </summary>
public sealed class UsersModule : IModule
{
    public string Name => "Users";
    public string Schema => UsersSchema.Name;

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<UsersDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, UsersSchema.Name);
                sql.EnableRetryOnFailure(maxRetryCount: 3);
            });

            options.AddInterceptors(
                sp.GetRequiredService<AuditingInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>());
        });

        // AddIdentityCore (not AddIdentity) - no cookie auth, no Razor UI, no Role
        // manager. Just what UserManager needs: hashing, lockout, and the token
        // providers behind email confirmation / password reset.
        services.AddIdentityCore<AppUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<UsersDbContext>()
            .AddDefaultTokenProviders(); // email-confirmation and password-reset tokens come from here

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IEmailSender, DevEmailSender>(); // swap for a real provider before deploying

        // NOTE: MediatR and FluentValidation are registered ONCE by the host,
        // same as every other module - see ModuleExtensions.AddModules.
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapAuthEndpoints();
    }
}
