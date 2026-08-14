using System;
using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using eShopWCFService;
using eShopWCFService.Models.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();

// Add EF Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("ConnectionString")
    ?? "Server=(localdb)\\mssqllocaldb;Database=eShopDatabase;Trusted_Connection=True;";

builder.Services.AddDbContext<EntityModel>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddTransient<CatalogService>();

var app = builder.Build();

// Seed database if possible
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<EntityModel>();
        db.Database.EnsureCreated();
        CatalogDBInitializer.Seed(db);
    }
}
catch
{
    // Database may not be available at startup
}

app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<CatalogService>(serviceOptions =>
    {
        serviceOptions.DebugBehavior.IncludeExceptionDetailInFaults = true;
    });

    serviceBuilder.AddServiceEndpoint<CatalogService, ICatalogService>(
        new CoreWCF.BasicHttpBinding(), "/CatalogService.svc");

    var serviceMetadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
    serviceMetadataBehavior.HttpGetEnabled = true;
});

app.Run();
