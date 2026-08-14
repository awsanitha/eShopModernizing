using System;
using System.Globalization;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopModernizedMVC;
using eShopModernizedMVC.Filters;
using eShopModernizedMVC.Middleware;
using eShopModernizedMVC.Models;
using eShopModernizedMVC.Models.Infrastructure;
using eShopModernizedMVC.Modules;
using eShopModernizedMVC.Services;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Initialize CatalogConfiguration static accessor with IConfiguration.
CatalogConfiguration.Initialize(builder.Configuration);

// log4net configuration (replaces AssemblyInfo XmlConfigurator attribute).
var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new System.IO.FileInfo(System.IO.Path.Combine(builder.Environment.ContentRootPath, "log4Net.xml")));

// Use Autofac as the DI container.
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// MVC with global filters (equivalent of FilterConfig.RegisterGlobalFilters).
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new ActionTracerFilter());
});

// Application Insights.
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    var key = builder.Configuration["AppInsightsInstrumentationKey"];
    if (!string.IsNullOrEmpty(key))
    {
        options.ConnectionString = $"InstrumentationKey={key}";
    }
});
builder.Services.AddSingleton<ITelemetryInitializer, MyTelemetryInitializer>();

// Authentication: Azure AD (OpenID Connect) or the simple custom pass-through middleware.
bool useAzureActiveDirectory = CatalogConfiguration.UseAzureActiveDirectory;

if (useAzureActiveDirectory)
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
        var aadInstance = CatalogConfiguration.AzureActiveDirectoryInstance;
        var tenant = CatalogConfiguration.AzureActiveDirectoryTenant;
        options.Authority = string.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
        options.SignedOutRedirectUri = CatalogConfiguration.PostLogoutRedirectUri;
        options.Events = new OpenIdConnectEvents
        {
            OnAuthenticationFailed = context =>
            {
                context.HandleResponse();
                context.Response.Redirect("/Error?message=" + context.Exception.Message);
                return System.Threading.Tasks.Task.CompletedTask;
            }
        };
    });
}
else
{
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie();
}
builder.Services.AddAuthorization();

// Register the Autofac module (equivalent of RegisterContainer() in Global.asax.cs).
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new ApplicationModule(
        CatalogConfiguration.UseMockData,
        CatalogConfiguration.UseAzureStorage,
        CatalogConfiguration.UseManagedIdentity));
});

var app = builder.Build();

// Equivalent of ConfigDataBase() + InitializeCatalogImages() + InitializePipeline() in Global.asax.cs.
using (var scope = app.Services.CreateScope())
{
    if (!CatalogConfiguration.UseMockData)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDBContext>();
        var initializer = scope.ServiceProvider.GetRequiredService<CatalogDBInitializer>();
        initializer.Initialize(dbContext);
    }

    var imageService = scope.ServiceProvider.GetRequiredService<IImageService>();
    imageService.InitializeCatalogImages();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Shared/Error");
}

app.UseStaticFiles();

app.UseRouting();

if (useAzureActiveDirectory)
{
    app.UseAuthentication();
}
else
{
    app.UseCustomAuthentication();
}

app.UseAuthorization();

app.MapControllerRoute(
    name: "Default",
    pattern: "{controller=Catalog}/{action=Index}/{id?}");

app.Run();
