using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Models.Infrastructure;
using eShopLegacyWebForms.Modules;
using eShopLegacyWebForms.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Autofac as DI container
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Add ASP.NET Core Razor Pages with MVC compatibility
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

// Configure Entity Framework Core
var useMockData = bool.Parse(builder.Configuration["AppSettings:UseMockData"] ?? "true");

if (!useMockData)
{
    builder.Services.AddDbContext<CatalogDBContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogDBContext")));
}

// Register app services in Autofac
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new ApplicationModule(useMockData));

    if (!useMockData)
    {
        containerBuilder.Register(c =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<CatalogDBContext>();
            optionsBuilder.UseSqlServer(builder.Configuration.GetConnectionString("CatalogDBContext"));
            return new CatalogDBContext(optionsBuilder.Options);
        }).AsSelf().InstancePerLifetimeScope();

        containerBuilder.RegisterType<CatalogDBInitializer>()
            .AsSelf()
            .InstancePerLifetimeScope();
    }
});

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

// Seed the database if not using mock data
if (!useMockData)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<CatalogDBContext>();
        var initializer = services.GetRequiredService<CatalogDBInitializer>();
        initializer.Seed(dbContext);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the database.");
    }
}

app.Run();
