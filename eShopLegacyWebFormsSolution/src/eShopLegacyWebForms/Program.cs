using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Models.Infrastructure;
using eShopLegacyWebForms.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Logging ──────────────────────────────────────────────────────────────────
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ── Razor Pages ──────────────────────────────────────────────────────────────
builder.Services.AddRazorPages()
    .AddRazorRuntimeCompilation();

// ── EF Core ──────────────────────────────────────────────────────────────────
var useMockData = builder.Configuration.GetValue<bool>("UseMockData");

if (!useMockData)
{
    builder.Services.AddDbContext<CatalogDBContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogDBContext")));
    builder.Services.AddScoped<CatalogItemHiLoGenerator>();
    builder.Services.AddScoped<ICatalogService, CatalogService>();
}
else
{
    // Mock uses no DB; register a singleton so it is shared across requests
    builder.Services.AddSingleton<ICatalogService, CatalogServiceMock>();
}

// ── Session (used by Site.Master for machine name / session start time) ──────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── DB initialisation (only when using real data) ────────────────────────────
if (!useMockData)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CatalogDBContext>();
    var hiLoGenerator = scope.ServiceProvider.GetRequiredService<CatalogItemHiLoGenerator>();
    var useCustomizationData = builder.Configuration.GetValue<bool>("UseCustomizationData");
    var webHostEnvironment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<CatalogDBInitializer>>();
    var initializer = new CatalogDBInitializer(hiLoGenerator, useCustomizationData, webHostEnvironment, logger);
    initializer.Seed(db);
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.MapRazorPages();

app.Run();
