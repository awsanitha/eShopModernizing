using CoreWCF;
using CoreWCF.Configuration;
using eShopWCFService;
using eShopWCFService.Models;
using eShopWCFService.Models.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure EF Core
var connectionString = Environment.GetEnvironmentVariable("ConnectionString")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=eShopDatabase;Persist Security Info=True;";

builder.Services.AddDbContext<EntityModel>(options =>
    options.UseSqlServer(connectionString));

// Register CoreWCF service model
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();

// Register the WCF service and dependencies
builder.Services.AddTransient<CatalogService>();

var app = builder.Build();

// Initialize the database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EntityModel>();
    context.Database.EnsureCreated();
    CatalogDBInitializer.SeedData(context);
}

app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<CatalogService>(serviceOptions =>
    {
        serviceOptions.DebugBehavior.IncludeExceptionDetailInFaults = app.Environment.IsDevelopment();
    });

    serviceBuilder.AddServiceEndpoint<CatalogService, ICatalogService>(
        new BasicHttpBinding(), "/CatalogService.svc");

    var serviceMetadataBehavior = app.Services.GetRequiredService<CoreWCF.Description.ServiceMetadataBehavior>();
    serviceMetadataBehavior.HttpGetEnabled = true;
});

app.Run();
