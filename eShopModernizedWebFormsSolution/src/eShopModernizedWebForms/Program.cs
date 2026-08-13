using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopModernizedWebForms;
using eShopModernizedWebForms.Middleware;
using eShopModernizedWebForms.Models;
using eShopModernizedWebForms.Models.Infrastructure;
using eShopModernizedWebForms.Modules;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Use Autofac as DI container
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Initialize configuration accessor
CatalogConfiguration.Initialize(builder.Configuration);

// Add MVC services
builder.Services.AddControllersWithViews();

// Add Application Insights
var appInsightsKey = builder.Configuration["AppInsightsInstrumentationKey"];
if (!string.IsNullOrEmpty(appInsightsKey))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = $"InstrumentationKey={appInsightsKey}";
    });
    builder.Services.AddSingleton<Microsoft.ApplicationInsights.Extensibility.ITelemetryInitializer, eShopModernizedWebForms.MyTelemetryInitializer>();
}

// Configure EF Core (only when not using mock data)
bool useMockData = builder.Configuration.GetValue<bool>("UseMockData");
bool useAzureStorage = builder.Configuration.GetValue<bool>("UseAzureStorage");
bool useManagedIdentity = builder.Configuration.GetValue<bool>("UseAzureManagedIdentity");
bool useAzureActiveDirectory = builder.Configuration.GetValue<bool>("UseAzureActiveDirectory");

if (!useMockData)
{
    var connectionString = builder.Configuration.GetConnectionString("CatalogDBContext")
        ?? "Server=(localdb)\\mssqllocaldb;Database=eShopModernizedWebFormsDB;Trusted_Connection=True;MultipleActiveResultSets=true";

    builder.Services.AddDbContext<CatalogDBContext>(options =>
        options.UseSqlServer(connectionString));
}

// Configure authentication
if (useAzureActiveDirectory)
{
    var clientId = builder.Configuration["AzureActiveDirectoryClientId"] ?? string.Empty;
    var tenant = builder.Configuration["AzureActiveDirectoryTenant"] ?? string.Empty;
    var aadInstance = builder.Configuration["AzureActiveDirectoryInstance"] ?? "https://login.microsoftonline.com/{0}";
    var postLogoutRedirectUri = builder.Configuration["PostLogoutRedirectUri"] ?? "/";

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(options =>
    {
        options.ClientId = clientId;
        options.Authority = string.Format(aadInstance, tenant);
        options.SignedOutRedirectUri = postLogoutRedirectUri;
        options.Events = new OpenIdConnectEvents
        {
            OnAuthenticationFailed = context =>
            {
                context.HandleResponse();
                context.Response.Redirect("/Error?message=" + context.Exception.Message);
                return Task.FromResult(0);
            }
        };
    });
}
else
{
    builder.Services.AddTransient<AuthenticationMiddleware>();
}

// Register Autofac modules
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new ApplicationModule(useMockData, useAzureStorage, useManagedIdentity));
});

// Configure log4net
log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4Net.xml"));

var app = builder.Build();

// Initialize the database
if (!useMockData)
{
    using var scope = app.Services.CreateScope();
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<CatalogDBContext>();
        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        context.Database.EnsureCreated();
        var initializer = new CatalogDBInitializer(
            scope.ServiceProvider.GetRequiredService<eShopModernizedWebForms.Models.CatalogItemHiLoGenerator>(),
            env);
        initializer.Seed(context);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
        logger?.LogError(ex, "An error occurred while initializing the database.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
// Also serve static files from Content, Scripts, Images directories (legacy MVC structure)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(app.Environment.ContentRootPath),
    RequestPath = ""
});

app.UseRouting();

if (useAzureActiveDirectory)
{
    app.UseAuthentication();
    app.UseAuthorization();
}
else
{
    app.UseMiddleware<AuthenticationMiddleware>();
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Catalog}/{action=Index}/{id?}");

app.Run();
