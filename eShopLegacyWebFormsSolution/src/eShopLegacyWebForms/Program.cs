using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Models.Infrastructure;
using eShopLegacyWebForms.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Pages
builder.Services.AddRazorPages();

// Read configuration
var useMockData = builder.Configuration.GetValue<bool>("UseMockData");

if (useMockData)
{
    builder.Services.AddSingleton<ICatalogService, CatalogServiceMock>();
}
else
{
    builder.Services.AddDbContext<CatalogDBContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogDBContext")));
    builder.Services.AddScoped<CatalogDBContext>();
    builder.Services.AddScoped<ICatalogService, CatalogService>();
    builder.Services.AddSingleton<CatalogItemHiLoGenerator>();
    builder.Services.AddScoped<CatalogDBInitializer>();
}

var app = builder.Build();

// Seed the database when not using mock data
if (!useMockData)
{
    using var scope = app.Services.CreateScope();
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<CatalogDBContext>();
        var initializer = scope.ServiceProvider.GetRequiredService<CatalogDBInitializer>();
        initializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating/seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Serve static files from project content root (preserves legacy paths: /Pics/, /Content/, /Scripts/, /images/, /fonts/)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(app.Environment.ContentRootPath),
    RequestPath = ""
});

// Serve from wwwroot if it exists
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
