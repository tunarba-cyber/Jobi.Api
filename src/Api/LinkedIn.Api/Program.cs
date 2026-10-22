using LinkedIn.Api.Extensions;
using LinkedIn.Api.Features.Contact;
using LinkedIn.Api.Middleware;
using LinkedIn.Modules.Users.Infrastructure.Tokens;
using LinkedIn.Shared.Abstractions.Security;
using LinkedIn.Shared.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

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

// Safety net: a crashing background service is logged and stopped, but cannot take the API down.
builder.Services.Configure<HostOptions>(o =>
    o.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);

builder.Services.AddLinkedInCors(builder.Configuration);

// Real client IP comes from the MVC server via X-Forwarded-For. Only loopback is
// trusted by default; in production add the MVC server's address to KnownProxies.
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor;
    // o.KnownProxies.Add(System.Net.IPAddress.Parse("10.0.0.5"));
});

// ---------------------------------------------------------------------------
// Authentication / Authorization
// ---------------------------------------------------------------------------
// JwtOptions is normally bound inside UsersModule.RegisterServices, but the
// signing key is needed here too, before that runs - read it directly from
// configuration for this one setup step.
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtSigningKey = jwtSection["SigningKey"];
if (string.IsNullOrWhiteSpace(jwtSigningKey) || jwtSigningKey.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:SigningKey must be at least 32 characters. Set it via user-secrets or an " +
        "environment variable - never commit a real signing key to appsettings.json. " +
        "Example: dotnet user-secrets set \"Jwt:SigningKey\" \"a-long-random-string-of-32+-chars\"");
}

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30) // small tolerance, not the 5-minute default
        };

        // SignalR (WebSockets) cannot send an Authorization header from a browser,
        // so the client sends the token as ?access_token=... instead. Accept it
        // from the query string ONLY for hub URLs - never for normal API routes,
        // because URLs end up in logs and caches.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthPolicies.Employer, p => p.RequireRole(AppRoles.Employer))
    .AddPolicy(AuthPolicies.Candidate, p => p.RequireRole(AppRoles.Candidate))
    .AddPolicy(AuthPolicies.Admin, p => p.RequireRole(AppRoles.Admin));

// Per-client (IP) limits for auth and contact; per-user limit for AI. Applied
// selectively via .RequireRateLimiting(...).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    static RateLimitPartition<string> PerIp(HttpContext ctx, int permits, TimeSpan window) =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permits,
                Window = window,
                QueueLimit = 0 // reject immediately over the limit, don't queue
            });

    options.AddPolicy(RateLimitPolicies.Auth, ctx => PerIp(ctx, 10, TimeSpan.FromMinutes(1)));
    options.AddPolicy(RateLimitPolicies.Contact, ctx => PerIp(ctx, 3, TimeSpan.FromMinutes(10)));

    // Per authenticated user (falls back to IP). Needs UseRateLimiter() to run
    // AFTER UseAuthentication(), otherwise ctx.User is empty here.
    options.AddPolicy(RateLimitPolicies.Ai, ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? ctx.User.FindFirstValue("sub")
                ?? ctx.Connection.RemoteIpAddress?.ToString()
                ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LinkedIn API",
        Version = "v1",
        Description = "Backend for the LinkedIn job board."
    });

    // Lets Swagger UI send "Authorization: Bearer <token>" on Try It Out calls.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste the JWT only. Swagger adds the 'Bearer ' prefix."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
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
app.UseForwardedHeaders();      // first: everything below sees the real client IP
app.UseExceptionHandler();      // wraps everything below

app.Use(async (ctx, next) =>
{
    var h = ctx.Response.Headers;
    h["X-Content-Type-Options"] = "nosniff";
    h["X-Frame-Options"] = "DENY";
    h["Referrer-Policy"] = "strict-origin-when-cross-origin";
    h["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.DocumentTitle = "LinkedIn API");
    await app.ApplyMigrationsAsync();
    await LinkedIn.Modules.Users.Infrastructure.AdminSeeder.SeedAsync(app.Services, app.Configuration);
    await LinkedIn.Api.Infrastructure.Seed.DataSeeder.SeedAsync(app.Services);
}
else
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseSerilogRequestLogging();
app.UseCors(CorsExtensions.FrontendPolicy);

app.UseAuthentication();
app.UseRateLimiter();           // after authentication so the AI policy can see the user
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapContactEndpoints();
app.MapModuleEndpoints(modules);

app.Logger.LogInformation(
    "LinkedIn API started with modules: {Modules}",
    string.Join(", ", modules.Select(m => m.Name)));

app.Run();