using LinkedIn.Api.Extensions;
using LinkedIn.Api.Middleware;
using LinkedIn.Modules.Users.Infrastructure.Tokens;
using LinkedIn.Shared.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

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

// ---------------------------------------------------------------------------
// Authentication / Authorization
// ---------------------------------------------------------------------------
// JwtOptions is normally bound inside UsersModule.RegisterServices, but the
// signing key is needed here too, before that runs - read it directly from
// configuration for this one setup step.
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtSigningKey = jwtSection["SigningKey"];
if (string.IsNullOrWhiteSpace(jwtSigningKey))
{
    throw new InvalidOperationException(
        "Jwt:SigningKey is not configured. Set it via user-secrets or an environment " +
        "variable - never commit a real signing key to appsettings.json. " +
        "Example: dotnet user-secrets set \"Jwt:SigningKey\" \"a-long-random-string\"");
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
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireEmployer", policy => policy.RequireRole("Employer"))
    .AddPolicy("RequireCandidate", policy => policy.RequireRole("Candidate"))
    .AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));

// Applied selectively (login/register/forgot-password only, via
// .RequireRateLimiting("auth") in AuthEndpoints) rather than globally - these
// three are the ones actually worth protecting from brute-force/spam.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = 5;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0; // reject immediately over the limit, don't queue
    });
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
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT}"
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
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapModuleEndpoints(modules);

app.Logger.LogInformation(
    "LinkedIn API started with modules: {Modules}",
    string.Join(", ", modules.Select(m => m.Name)));

app.Run();
