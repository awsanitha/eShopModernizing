using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using eShopWCFService;
using eShopWCFService.Models.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Configure EF Core with SQL Server
builder.Services.AddDbContext<EntityModel>(options =>
    options.UseSqlServer(
        System.Environment.GetEnvironmentVariable("ConnectionString")
        ?? builder.Configuration.GetConnectionString("EntityModel")
        ?? "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=eShopDatabase;Persist Security Info=True;"));

// Add CoreWCF services
builder.Services.AddServiceModelServices()
    .AddServiceModelMetadata();

// Register CatalogService for DI (transient so each WCF call gets its own instance)
builder.Services.AddTransient<CatalogService>();

var app = builder.Build();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EntityModel>();
    db.Database.EnsureCreated();
    CatalogDBInitializer.Seed(db);
}

// Configure CoreWCF service endpoints
app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<CatalogService>(serviceOptions =>
    {
        serviceOptions.DebugBehavior.IncludeExceptionDetailInFaults = false;
    });

    serviceBuilder.AddServiceEndpoint<CatalogService, ICatalogService>(
        new BasicHttpBinding(), "/CatalogService.svc");

    serviceBuilder.ConfigureServiceHostBase<CatalogService>(host =>
    {
        var metaBehavior = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
        if (metaBehavior != null)
            metaBehavior.HttpGetEnabled = true;
    });
});

app.Run();
