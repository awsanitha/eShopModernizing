using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.Modules;
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

bool useMockData = builder.Configuration.GetValue<bool>("UseMockData");

// Use Autofac as DI container
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Services.AddControllersWithViews();
builder.Services.AddSession();

if (!useMockData)
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
    builder.Services.AddDbContext<CatalogDBContext>(options =>
        options.UseSqlServer(connectionString));
}

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new ApplicationModule(useMockData));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Catalog}/{action=Index}/{id?}");

// Initialize database
if (!useMockData)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetService<CatalogDBContext>();
    if (dbContext != null)
    {
        dbContext.Database.EnsureCreated();
    }
}

app.Run();
