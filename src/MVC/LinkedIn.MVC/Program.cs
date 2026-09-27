using LinkedIn.MVC.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.Configure<JobiApiOptions>(builder.Configuration.GetSection(JobiApiOptions.SectionName));

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthTokenHandler>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
    });

// Same-origin raw client used ONLY by AuthTokenHandler to call refresh without recursing.
builder.Services.AddHttpClient("JobiApiRaw", (sp, http) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<JobiApiOptions>>().Value;
    http.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    http.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

builder.Services.AddHttpClient<IJobiApiClient, JobiApiClient>((sp, http) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<JobiApiOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.BaseUrl))
        throw new InvalidOperationException("JobiApi:BaseUrl is not configured.");
    http.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    http.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
})
.AddHttpMessageHandler<AuthTokenHandler>();   // <-- attaches JWT + auto-refresh to every call

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();   // <-- must come before UseAuthorization, and before MapControllerRoute
app.UseAuthorization();

app.MapControllerRoute(name: "jobDetails", pattern: "Job/JobDetails/{slug}", defaults: new { controller = "Job", action = "JobDetails" });
app.MapControllerRoute(name: "blogDetails", pattern: "Blog/Details/{slug}", defaults: new { controller = "Blog", action = "Details" });
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();