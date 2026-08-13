using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.Modules;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Use Autofac as DI container
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Add MVC services
builder.Services.AddControllersWithViews();

// Configuration
var useMockData = builder.Configuration.GetValue<bool>("UseMockData");

if (!useMockData)
{
    var connectionString = builder.Configuration.GetConnectionString("CatalogDBContext")
        ?? "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=eShopModernizedMVCDB;Trusted_Connection=True;MultipleActiveResultSets=true";

    builder.Services.AddDbContext<CatalogDBContext>(options =>
        options.UseSqlServer(connectionString));
}

// Register Autofac modules
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new ApplicationModule(useMockData));
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
            scope.ServiceProvider.GetRequiredService<CatalogItemHiLoGenerator>(),
            env,
            builder.Configuration);
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
app.UseAuthorization();

app.MapControllerRoute(
    name: "pic",
    pattern: "items/{catalogItemId:int}/pic",
    defaults: new { controller = "Pic", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Catalog}/{action=Index}/{id?}");

app.Run();
