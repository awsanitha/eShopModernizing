using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

// Configure log4net
log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4Net.xml"));

var builder = WebApplication.CreateBuilder(args);

// Use Autofac as the DI container
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

bool useMockData = builder.Configuration.GetValue<bool>("UseMockData");

// Register MVC with views
builder.Services.AddControllersWithViews();

// Register session
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

// Register EF Core DbContext (only when not using mock data)
if (!useMockData)
{
    string connectionString = builder.Configuration.GetConnectionString("CatalogDBContext");
    builder.Services.AddDbContext<CatalogDBContext>(options =>
        options.UseSqlServer(connectionString));
}

// Register Autofac modules
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new ApplicationModule(useMockData));
});

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

app.UseRouting();
app.UseSession();

// Attribute routes (e.g. PicController's [Route("items/{id}/pic")])
app.MapControllers();

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
            var initializer = scope.ServiceProvider.GetRequiredService<CatalogDBInitializer>();
            initializer.Initialize(scope.ServiceProvider);
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while initializing the database.");
        }
    }
}

app.Run();
