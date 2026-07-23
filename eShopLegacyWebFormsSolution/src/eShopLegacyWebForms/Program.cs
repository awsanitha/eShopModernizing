using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Models.Infrastructure;
using eShopLegacyWebForms.Modules;
using eShopLegacyWebForms.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Use Autofac as the DI container (preserves existing ApplicationModule)
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Add Razor Pages
builder.Services.AddRazorPages();

// Register EF Core DbContext
var connectionString = builder.Configuration.GetConnectionString("CatalogDBContext");
builder.Services.AddDbContext<CatalogDBContext>(options =>
    options.UseSqlServer(connectionString));

// Configure Autofac modules
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    var useMockData = bool.Parse(builder.Configuration["AppSettings:UseMockData"] ?? "true");
    containerBuilder.RegisterModule(new ApplicationModule(useMockData));
});

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure log4net
builder.Logging.AddLog4Net("log4net.xml");

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

// Initialize the database on startup when not using mock data
var useMockDataForInit = bool.Parse(app.Configuration["AppSettings:UseMockData"] ?? "true");
if (!useMockDataForInit)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDBContext>();
    var initializer = scope.ServiceProvider.GetRequiredService<CatalogDBInitializer>();
    await initializer.InitializeAsync(dbContext);
}

app.Run();
