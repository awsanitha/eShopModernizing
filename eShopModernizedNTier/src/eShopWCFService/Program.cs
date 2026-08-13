using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using eShopWCFService;
using eShopWCFService.Models.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure EF Core
var connectionString = Environment.GetEnvironmentVariable("ConnectionString")
    ?? builder.Configuration.GetConnectionString("EntityModel")
    ?? "Server=(localdb)\\mssqllocaldb;Database=eShopCatalog;Trusted_Connection=True;MultipleActiveResultSets=true";

builder.Services.AddDbContext<EntityModel>(options =>
    options.UseSqlServer(connectionString));

// Register CatalogService for DI (CoreWCF will resolve from DI container)
builder.Services.AddScoped<CatalogService>();

// Add CoreWCF services
builder.Services.AddServiceModelServices()
    .AddServiceModelMetadata();

var app = builder.Build();

// Seed the database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EntityModel>();
    CatalogDBInitializer.Initialize(db);
}

// Configure CoreWCF endpoints
app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<CatalogService>(options =>
    {
        options.DebugBehavior.IncludeExceptionDetailInFaults = false;
    });

    serviceBuilder.AddServiceEndpoint<CatalogService, ICatalogService>(
        new BasicHttpBinding
        {
            MaxBufferSize = int.MaxValue,
            MaxReceivedMessageSize = int.MaxValue
        },
        "/CatalogService.svc");

    serviceBuilder.ConfigureServiceHostBase<CatalogService>(host =>
    {
        var metaBehavior = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
        if (metaBehavior == null)
        {
            metaBehavior = new ServiceMetadataBehavior();
            host.Description.Behaviors.Add(metaBehavior);
        }
        metaBehavior.HttpGetEnabled = true;
    });
});

app.Run();
