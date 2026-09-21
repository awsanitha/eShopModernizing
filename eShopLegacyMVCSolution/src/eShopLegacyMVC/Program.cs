using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.Modules;
using log4net;
using log4net.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Configure log4net
var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly()!);
XmlConfigurator.Configure(logRepository, new FileInfo(
    Path.Combine(builder.Environment.ContentRootPath, "log4net.xml")));

var useMockData = builder.Configuration.GetValue<bool>("UseMockData");

// Use Autofac as the DI container
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddDistributedMemoryCache();

if (!useMockData)
{
    builder.Services.AddDbContext<CatalogDBContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogDBContext")));
}

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new ApplicationModule(useMockData));
});

var app = builder.Build();

// Seed the database at startup
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
        var logger = LogManager.GetLogger(typeof(Program));
        logger.Error("Error seeding database", ex);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Shared/Error");
}

// Serve static files from wwwroot (default)
app.UseStaticFiles();

// Serve from project content directories to match legacy ~/Content/, ~/Scripts/, ~/Images/ paths
var contentRoot = app.Environment.ContentRootPath;
foreach (var dir in new[] { ("Content", "/Content"), ("Scripts", "/Scripts"), ("Images", "/Images"), ("images", "/images"), ("fonts", "/fonts") })
{
    var physicalPath = Path.Combine(contentRoot, dir.Item1);
    if (Directory.Exists(physicalPath))
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(physicalPath),
            RequestPath = dir.Item2
        });
    }
}

// Also serve Images/ at /images/ path (case-insensitive compat)
var imagesPath = Path.Combine(contentRoot, "Images");
if (Directory.Exists(imagesPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(imagesPath),
        RequestPath = "/images"
    });
}

app.UseRouting();
app.UseSession();

// Replaces Session_Start: initialize session values on first request
app.Use(async (context, next) =>
{
    if (string.IsNullOrEmpty(context.Session.GetString("MachineName")))
    {
        context.Session.SetString("MachineName", Environment.MachineName);
        context.Session.SetString("SessionStartTime", DateTime.Now.ToString("G"));
    }
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Catalog}/{action=Index}/{id?}");

app.MapControllers();

app.Run();
