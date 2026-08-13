using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Models.Infrastructure;
using eShopLegacyWebForms.Modules;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Use Autofac as DI container
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Add MVC services
builder.Services.AddControllersWithViews();

var useMockData = builder.Configuration.GetValue<bool>("UseMockData");

if (!useMockData)
{
    var connectionString = builder.Configuration.GetConnectionString("CatalogDBContext")
        ?? "Server=(localdb)\\mssqllocaldb;Database=eShopLegacyWebFormsDB;Trusted_Connection=True;MultipleActiveResultSets=true";

    builder.Services.AddDbContext<CatalogDBContext>(options =>
        options.UseSqlServer(connectionString));
}

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
            scope.ServiceProvider.GetRequiredService<eShopLegacyWebForms.Models.CatalogItemHiLoGenerator>(),
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
    app.UseDeveloperExceptionPage();
else
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
// Also serve static files from legacy directories (images, Pics, Content, Scripts)
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(app.Environment.ContentRootPath),
    RequestPath = ""
});

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Catalog}/{action=Index}/{id?}");

app.Run();
