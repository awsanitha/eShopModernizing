using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Models.Infrastructure;
using eShopLegacyWebForms.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// Configuration values
var useMockData = builder.Configuration.GetValue<bool>("AppSettings:UseMockData");
var useCustomizationData = builder.Configuration.GetValue<bool>("AppSettings:UseCustomizationData");

// Entity Framework Core - only register when not using mock data
if (!useMockData)
{
    builder.Services.AddDbContext<CatalogDBContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogDBContext")));
}

// Catalog service
if (useMockData)
{
    builder.Services.AddSingleton<ICatalogService, CatalogServiceMock>();
}
else
{
    builder.Services.AddScoped<ICatalogService, CatalogService>();
    builder.Services.AddScoped<CatalogDBContext>();
    builder.Services.AddScoped<CatalogDBInitializer>(sp =>
        new CatalogDBInitializer(
            sp.GetRequiredService<CatalogItemHiLoGenerator>(),
            sp.GetRequiredService<IHostEnvironment>(),
            useCustomizationData));
    builder.Services.AddSingleton<CatalogItemHiLoGenerator>();
}

// ── App pipeline ──────────────────────────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Serve static files from the content root (legacy paths: Content/, Scripts/, images/, Pics/, fonts/)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(app.Environment.ContentRootPath),
    RequestPath = string.Empty
});

// Also serve from wwwroot if it exists
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Catalog}/{action=Index}/{id?}");

// Seed database if not using mock data
if (!useMockData)
{
    using var scope = app.Services.CreateScope();
    try
    {
        var initializer = scope.ServiceProvider.GetRequiredService<CatalogDBInitializer>();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDBContext>();
        initializer.Seed(context);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
