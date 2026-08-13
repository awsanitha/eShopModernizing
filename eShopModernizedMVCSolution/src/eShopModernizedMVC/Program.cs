using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopModernizedMVC;
using eShopModernizedMVC.Filters;
using eShopModernizedMVC.Middleware;
using eShopModernizedMVC.Models;
using eShopModernizedMVC.Modules;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Initialize CatalogConfiguration with the configuration
CatalogConfiguration.Initialize(builder.Configuration);

// Use Autofac as the DI container
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Add services
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ActionTracerFilter>();
});

// Configure App Insights
builder.Services.AddApplicationInsightsTelemetry();

if (!CatalogConfiguration.UseMockData)
{
    var connectionString = builder.Configuration.GetConnectionString("CatalogDBContext");
    builder.Services.AddDbContext<CatalogDBContext>(options =>
        options.UseSqlServer(connectionString));
}

// Configure Authentication
if (CatalogConfiguration.UseAzureActiveDirectory)
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(options =>
    {
        options.ClientId = CatalogConfiguration.AzureActiveDirectoryClientId;
        options.Authority = string.Format(
            System.Globalization.CultureInfo.InvariantCulture,
            builder.Configuration["AzureActiveDirectoryInstance"] ?? "https://login.microsoftonline.com/{0}/",
            CatalogConfiguration.AzureActiveDirectoryTenant);
        options.SignedOutRedirectUri = CatalogConfiguration.PostLogoutRedirectUri;
        options.CallbackPath = "/signin-oidc";
    });
}
else
{
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie();
}

builder.Services.AddSession();

// Configure Autofac container
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new ApplicationModule(
        CatalogConfiguration.UseMockData,
        CatalogConfiguration.UseAzureStorage));
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseSession();
app.UseRouting();

if (CatalogConfiguration.UseAzureActiveDirectory)
{
    app.UseAuthentication();
    app.UseAuthorization();
}
else
{
    app.UseMiddleware<AuthenticationMiddleware>();
    app.UseAuthorization();
}

// Initialize Application Insights telemetry
var telemetryConfig = app.Services.GetService<TelemetryConfiguration>();
if (telemetryConfig != null)
{
    telemetryConfig.TelemetryInitializers.Add(new MyTelemetryInitializer());
    if (!string.IsNullOrEmpty(CatalogConfiguration.AppInsightsInstrumentationKey))
    {
        telemetryConfig.InstrumentationKey = CatalogConfiguration.AppInsightsInstrumentationKey;
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Catalog}/{action=Index}/{id?}");

app.Run();
