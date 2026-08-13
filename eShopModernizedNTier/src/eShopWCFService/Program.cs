using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using Microsoft.EntityFrameworkCore;
using eShopWCFService;
using eShopWCFService.Models.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Register EF Core DbContext
var connectionString = builder.Configuration.GetConnectionString("EntityModel")
    ?? Environment.GetEnvironmentVariable("ConnectionString")
    ?? CatalogConfiguration.ConnectionString;

builder.Services.AddDbContext<EntityModel>(options =>
    options.UseSqlServer(connectionString));

// Register CatalogService for DI / CoreWCF
builder.Services.AddTransient<CatalogService>();

// Register CoreWCF services
builder.Services.AddServiceModelServices()
    .AddServiceModelMetadata();

var app = builder.Build();

// Initialize the database (EnsureCreated + seed) on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<EntityModel>();
        CatalogDBInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        // Log and continue — DB may not be available in all environments (e.g., unit tests)
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Database initialization failed. The application will start without a seeded database.");
    }
}

// Configure CoreWCF endpoints
app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<CatalogService>(serviceOptions =>
    {
        serviceOptions.DebugBehavior.IncludeExceptionDetailInFaults = true;
    });

    var basicHttpBinding = new BasicHttpBinding
    {
        MaxBufferSize = int.MaxValue,
        MaxReceivedMessageSize = int.MaxValue
    };

    serviceBuilder.AddServiceEndpoint<CatalogService, ICatalogService>(
        basicHttpBinding, "/CatalogService.svc");

    serviceBuilder.ConfigureServiceHostBase<CatalogService>(host =>
    {
        var metadataBehavior = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
        if (metadataBehavior == null)
        {
            metadataBehavior = new ServiceMetadataBehavior { HttpGetEnabled = true };
            host.Description.Behaviors.Add(metadataBehavior);
        }
        else
        {
            metadataBehavior.HttpGetEnabled = true;
        }
    });
});

app.Run();
