using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add MVC services
builder.Services.AddControllersWithViews();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

// Configure data access
bool useMockData = builder.Configuration.GetValue<bool>("UseMockData");

if (!useMockData)
{
    string connectionString = builder.Configuration.GetConnectionString("CatalogDBContext");
    builder.Services.AddDbContext<CatalogDBContext>(options =>
        options.UseSqlServer(connectionString));
    builder.Services.AddScoped<ICatalogService, CatalogService>();
    builder.Services.AddScoped<CatalogDBInitializer>();
}
else
{
    builder.Services.AddSingleton<ICatalogService, CatalogServiceMock>();
}

builder.Services.AddSingleton<CatalogItemHiLoGenerator>();

// Configure log4net
log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4Net.xml"));

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

app.UseStaticFiles();

// Serve legacy static file paths from content root
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        System.IO.Path.Combine(builder.Environment.ContentRootPath, "Content")),
    RequestPath = "/Content"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        System.IO.Path.Combine(builder.Environment.ContentRootPath, "Scripts")),
    RequestPath = "/Scripts"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        System.IO.Path.Combine(builder.Environment.ContentRootPath, "Images")),
    RequestPath = "/images"
});
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        System.IO.Path.Combine(builder.Environment.ContentRootPath, "fonts")),
    RequestPath = "/fonts"
});

app.UseSession();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Catalog}/{action=Index}/{id?}");

// Initialize database on startup (if not using mock data)
if (!useMockData)
{
    using var scope = app.Services.CreateScope();
    try
    {
        var initializer = scope.ServiceProvider.GetRequiredService<CatalogDBInitializer>();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDBContext>();
        initializer.InitializeDatabase(context);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

app.Run();
