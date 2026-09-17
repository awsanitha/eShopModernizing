using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using eShopWCFService;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

var builder = WebApplication.CreateBuilder(args);

// Register EF Core DbContext
builder.Services.AddDbContext<EntityModel>(options =>
{
    var connStr = Environment.GetEnvironmentVariable("ConnectionString")
        ?? builder.Configuration.GetConnectionString("EntityModel")
        ?? "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=eShopDatabase;Persist Security Info=True;";
    options.UseSqlServer(connStr);
});

// Register the WCF service as transient so CoreWCF can create instances per-call
builder.Services.AddTransient<CatalogService>();

// Add CoreWCF service infrastructure
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();

var app = builder.Build();

// Ensure database is created and seed data is applied
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EntityModel>();
    db.Database.EnsureCreated();
}

// Configure CoreWCF service endpoints
app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<CatalogService>(serviceOptions =>
    {
        serviceOptions.DebugBehavior.IncludeExceptionDetailInFaults = true;
    });

    serviceBuilder.AddServiceEndpoint<CatalogService, ICatalogService>(
        new BasicHttpBinding(),
        "/CatalogService.svc");

    serviceBuilder.ConfigureServiceHostBase<CatalogService>(host =>
    {
        var metaBehavior = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
        if (metaBehavior != null)
        {
            metaBehavior.HttpGetEnabled = true;
        }
    });
});

app.Run();
