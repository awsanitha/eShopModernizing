using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopModernizedWebForms.Models;
using eShopModernizedWebForms.Modules;
using eShopModernizedWebForms.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

bool useMockData = builder.Configuration.GetValue<bool>("UseMockData");
bool useAzureStorage = builder.Configuration.GetValue<bool>("UseAzureStorage");
bool useManagedIdentity = builder.Configuration.GetValue<bool>("UseAzureManagedIdentity");

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Services.AddControllersWithViews();
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
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Catalog}/{action=Index}/{id?}");

// Initialize catalog images
using (var scope = app.Services.CreateScope())
{
    var imageService = scope.ServiceProvider.GetService<IImageService>();
    imageService?.InitializeCatalogImages();
}

if (!useMockData)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetService<CatalogDBContext>();
    dbContext?.Database.EnsureCreated();
}

app.Run();
