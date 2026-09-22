using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

IConfiguration config = builder.Configuration;
bool useMockData = config.GetValue<bool>("UseMockData");

builder.Services.AddControllersWithViews();
builder.Services.AddSession();

if (!useMockData)
{
    string connectionString = config.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<CatalogDBContext>(options =>
        options.UseSqlServer(connectionString));
}

builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
    container.RegisterModule(new ApplicationModule(useMockData));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseExceptionHandler("/Catalog/Error");

app.UseSession();
app.UseStaticFiles();

app.Use(async (context, next) =>
{
    log4net.LogicalThreadContext.Properties["activityid"] = new eShopLegacyMVC.ActivityIdHelper();
    await next();
});

app.UseRouting();
app.MapControllerRoute("default", "{controller=Catalog}/{action=Index}/{id?}");
app.MapControllers();

app.Run();
