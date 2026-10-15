using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Models.Infrastructure;
using eShopLegacyWebForms.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Pages
builder.Services.AddRazorPages();

// Configure EF Core
var connectionString = builder.Configuration.GetConnectionString("CatalogDBContext");
builder.Services.AddDbContext<CatalogDBContext>(options =>
    options.UseSqlServer(connectionString));

// Configure catalog service based on UseMockData setting
var useMockData = bool.Parse(builder.Configuration["AppSettings:UseMockData"] ?? "true");
if (useMockData)
{
    builder.Services.AddSingleton<ICatalogService, CatalogServiceMock>();
}
else
{
    builder.Services.AddScoped<ICatalogService, CatalogService>();
    builder.Services.AddSingleton<CatalogItemHiLoGenerator>();
}

// Configure session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Seed the database on startup (only when not using mock data)
if (!useMockData)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<CatalogDBContext>();
    var hiLoGenerator = scope.ServiceProvider.GetRequiredService<CatalogItemHiLoGenerator>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var initializer = new CatalogDBInitializer(hiLoGenerator, configuration, env);
        initializer.Seed(context);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapRazorPages();

// Set session data at request start (equivalent of Session_Start in Global.asax)
app.Use(async (context, next) =>
{
    if (context.Session.GetString("MachineName") == null)
    {
        context.Session.SetString("MachineName", Environment.MachineName);
        context.Session.SetString("SessionStartTime", DateTime.Now.ToString("O"));
    }
    await next(context);
});

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
