using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopModernizedMVC;
using eShopModernizedMVC.Models;
using eShopModernizedMVC.Models.Infrastructure;
using eShopModernizedMVC.Modules;
using eShopModernizedMVC.Services;
using log4net;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Configure log4net
var logRepo = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly()!);
log4net.Config.XmlConfigurator.Configure(logRepo, new System.IO.FileInfo("log4Net.xml"));

var builder = WebApplication.CreateBuilder(args);

CatalogConfiguration.Initialize(builder.Configuration);

bool useMockData = builder.Configuration.GetValue<bool>("UseMockData");
bool useAzureStorage = builder.Configuration.GetValue<bool>("UseAzureStorage");
bool useManagedIdentity = builder.Configuration.GetValue<bool>("UseManagedIdentity");

// Use Autofac as DI container
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Services.AddControllersWithViews();

builder.Services.AddSession();
builder.Services.AddApplicationInsightsTelemetry();

if (!useMockData)
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
    builder.Services.AddDbContext<CatalogDBContext>(options =>
        options.UseSqlServer(connectionString));
}

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new ApplicationModule(useMockData, useAzureStorage, useManagedIdentity));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Catalog}/{action=Index}/{id?}");

// Initialize catalog images
using (var scope = app.Services.CreateScope())
{
    var imageService = scope.ServiceProvider.GetService<IImageService>();
    imageService?.InitializeCatalogImages();
    if (!useMockData)
    {
        var dbContext = scope.ServiceProvider.GetService<CatalogDBContext>();
        if (dbContext != null)
        {
            dbContext.Database.EnsureCreated();
        }
    }
}

app.Run();
