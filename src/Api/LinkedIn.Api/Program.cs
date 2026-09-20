using LinkedIn.Api.Extensions;
using LinkedIn.Api.Middleware;
using LinkedIn.Shared.Infrastructure.Persistence;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Logging
// ---------------------------------------------------------------------------
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// ---------------------------------------------------------------------------
// Cross-cutting services
// ---------------------------------------------------------------------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// Interceptors are resolved per module DbContext registration.
builder.Services.AddScoped<AuditingInterceptor>();
builder.Services.AddScoped<SoftDeleteInterceptor>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddLinkedInCors(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LinkedIn API",
        Version = "v1",
        Description = "Backend for the LinkedIn job board."
    });
});

builder.Services.AddHealthChecks();

// ---------------------------------------------------------------------------
// Modules - discovered by convention, registered in one pass
// ---------------------------------------------------------------------------
var modules = builder.Services.DiscoverModules();
builder.Services.AddModules(builder.Configuration, modules);

var app = builder.Build();

// ---------------------------------------------------------------------------
// Pipeline (order matters)
// ---------------------------------------------------------------------------
app.UseExceptionHandler();      // must be first so it wraps everything below

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.DocumentTitle = "LinkedIn API");
    await app.ApplyMigrationsAsync();
}
else
{
    app.UseHttpsRedirection();
}

app.UseSerilogRequestLogging();
app.UseCors(CorsExtensions.FrontendPolicy);

app.MapHealthChecks("/health");
app.MapModuleEndpoints(modules);

app.Logger.LogInformation(
    "LinkedIn API started with modules: {Modules}",
    string.Join(", ", modules.Select(m => m.Name)));

app.Run();
