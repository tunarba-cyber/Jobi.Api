using LinkedIn.Modules.Notifications.Features;
using LinkedIn.Modules.Notifications.Infrastructure;
using LinkedIn.Shared.Abstractions.Contracts;
using LinkedIn.Shared.Abstractions.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinkedIn.Modules.Notifications;

public sealed class NotificationsModule : IModule
{
    public string Name => "Notifications";
    public string Schema => NotificationsDbContext.Schema;

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<NotificationsDbContext>(o => o.UseSqlServer(cs, sql =>
        {
            sql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, NotificationsDbContext.Schema);
            sql.EnableRetryOnFailure(3);
        }));

        services.AddScoped<INotificationPublisher, NotificationPublisher>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints) => endpoints.MapNotificationEndpoints();
}