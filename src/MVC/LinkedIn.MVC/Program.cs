using LinkedIn.MVC.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(o =>
    o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

builder.Services.AddAntiforgery(o =>
{
    o.HeaderName = "RequestVerificationToken";
    o.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    o.HeaderName = "RequestVerificationToken"; // lets fetch/AJAX send the token as a header
});

builder.Services.Configure<JobiApiOptions>(builder.Configuration.GetSection(JobiApiOptions.SectionName));

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthTokenHandler>();
builder.Services.AddTransient<ForwardClientIpHandler>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "jobi.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.LoginPath = "/";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
        options.AccessDeniedPath = "/Error/403";
    });

// Same-origin raw client used ONLY by AuthTokenHandler to call refresh without recursing.
builder.Services.AddHttpClient("JobiApiRaw", (sp, http) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<JobiApiOptions>>().Value;
    http.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    http.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
})
.AddHttpMessageHandler<ForwardClientIpHandler>();

builder.Services.AddHttpClient<IJobiApiClient, JobiApiClient>((sp, http) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<JobiApiOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.BaseUrl))
        throw new InvalidOperationException("JobiApi:BaseUrl is not configured.");
    http.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    http.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
})
.AddHttpMessageHandler<AuthTokenHandler>()      // attaches JWT + auto-refresh to every call
.AddHttpMessageHandler<ForwardClientIpHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseHttpsRedirection();

app.Use(async (ctx, next) =>
{
    var h = ctx.Response.Headers;
    h["X-Content-Type-Options"] = "nosniff";
    h["X-Frame-Options"] = "DENY";
    h["Referrer-Policy"] = "strict-origin-when-cross-origin";
    h["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next();
});

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "jobDetails", pattern: "Job/JobDetails/{slug}", defaults: new { controller = "Job", action = "JobDetails" });
app.MapControllerRoute(
    name: "blogDetails",
    pattern: "Blog/Details/{id:guid}",
    defaults: new { controller = "Blog", action = "Details" });
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();