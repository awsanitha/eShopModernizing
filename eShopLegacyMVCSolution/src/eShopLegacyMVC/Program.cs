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

// Use Autofac as the DI container
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Add MVC services
builder.Services.AddControllersWithViews();

// Configure EF Core if not using mock data
bool useMockData = builder.Configuration.GetValue<bool>("UseMockData");
if (!useMockData)
{
    string? connectionString = builder.Configuration.GetConnectionString("CatalogDBContext");
    builder.Services.AddDbContext<CatalogDBContext>(options =>
        options.UseSqlServer(connectionString));
}

// Configure Autofac container
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new ApplicationModule(useMockData));
});

var app = builder.Build();

// Configure the HTTP request pipeline
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Catalog}/{action=Index}/{id?}");

app.MapControllers();

app.Run();
