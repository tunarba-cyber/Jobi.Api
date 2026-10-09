using LinkedIn.Modules.Messaging.Features;
using LinkedIn.Modules.Messaging.Infrastructure.Persistence;
using LinkedIn.Modules.Messaging.Infrastructure.Realtime;
using LinkedIn.Shared.Abstractions.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinkedIn.Modules.Messaging;

/// <summary>Self-registering module - same contract as UsersModule / JobsModule.</summary>
public sealed class MessagingModule : IModule
{
    public string Name => "Messaging";
    public string Schema => MessagingSchema.Name;

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<MessagingDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, MessagingSchema.Name);
                sql.EnableRetryOnFailure(maxRetryCount: 3);
            }));

        services.AddSignalR();
        services.AddSingleton<IUserIdProvider, SubClaimUserIdProvider>();
        services.AddScoped<IMessageNotifier, SignalRMessageNotifier>();

        // MediatR / FluentValidation are registered ONCE by the host (ModuleExtensions.AddModules).
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapMessagingEndpoints();
        endpoints.MapHub<ChatHub>("/hubs/chat");
        endpoints.MapAdminContactEndpoints();
    }
}
