using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using eShopWCFService;
using eShopWCFService.Models.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Resolve connection string: environment variable takes precedence, then appsettings.json
var connectionString = Environment.GetEnvironmentVariable("ConnectionString")
    ?? builder.Configuration.GetConnectionString("EntityModel")
    ?? "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=eShopDatabase;Persist Security Info=True;";

// Register EF Core DbContext
builder.Services.AddDbContext<EntityModel>(options =>
    options.UseSqlServer(connectionString));

// Register the WCF service implementation (scoped so it shares DbContext lifetime)
builder.Services.AddScoped<CatalogService>();

// Add CoreWCF services
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();

var app = builder.Build();

// Initialize and seed the database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EntityModel>();
    db.Database.EnsureCreated();
    CatalogDBInitializer.Seed(db);
}

// Configure the CoreWCF endpoints
app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<CatalogService>(serviceOptions =>
    {
        serviceOptions.DebugBehavior.IncludeExceptionDetailInFaults = true;
    });

    // Expose the service at /CatalogService.svc (matching legacy URL for client compatibility)
    serviceBuilder.AddServiceEndpoint<CatalogService, ICatalogService>(
        new BasicHttpBinding(),
        "/CatalogService.svc");

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
