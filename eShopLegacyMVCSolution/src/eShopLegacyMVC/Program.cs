using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.Services;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configure log4net
var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo(Path.Combine(builder.Environment.ContentRootPath, "log4Net.xml")));

// Add MVC with views
builder.Services.AddControllersWithViews();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure EF Core and application services
bool useMockData = builder.Configuration.GetValue<bool>("UseMockData");

if (!useMockData)
{
    builder.Services.AddDbContext<CatalogDBContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogDBContext")));
    builder.Services.AddScoped<ICatalogService, CatalogService>();
    builder.Services.AddSingleton<CatalogItemHiLoGenerator>();
    builder.Services.AddScoped<CatalogDBInitializer>();
}
else
{
    builder.Services.AddSingleton<ICatalogService, CatalogServiceMock>();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Catalog/Error");
    app.UseHsts();
}

// Serve static files from legacy content directories
var contentRoot = builder.Environment.ContentRootPath;

void TryAddStaticFiles(string dirName, string requestPath)
{
    var dirPath = Path.Combine(contentRoot, dirName);
    if (Directory.Exists(dirPath))
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(dirPath),
            RequestPath = requestPath
        });
    }
}

TryAddStaticFiles("Content", "/Content");
TryAddStaticFiles("Scripts", "/Scripts");
TryAddStaticFiles("Images", "/Images");
TryAddStaticFiles("Pics", "/Pics");
TryAddStaticFiles("fonts", "/fonts");

// Serve favicon.ico from content root
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(contentRoot),
    RequestPath = ""
});

app.UseRouting();
app.UseSession();

// Attribute-routed controllers (e.g. PicController)
app.MapControllers();

// Conventional routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Catalog}/{action=Index}/{id?}");

// Initialize database if not using mock data
if (!useMockData)
{
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDBContext>();
            var initializer = scope.ServiceProvider.GetRequiredService<CatalogDBInitializer>();
            initializer.Initialize(dbContext, app.Environment.ContentRootPath,
                builder.Configuration.GetValue<bool>("UseCustomizationData"));
        }
        catch (Exception ex)
        {
            var log = LogManager.GetLogger(typeof(Program));
            log.Error("Error initializing database", ex);
        }
    }
}

app.Run();
