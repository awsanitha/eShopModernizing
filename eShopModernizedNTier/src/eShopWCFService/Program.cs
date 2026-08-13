using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using eShopWCFService;
using eShopWCFService.Models.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Resolve connection string: env var overrides appsettings.json
var connectionString = CatalogConfiguration.ConnectionString
    ?? builder.Configuration.GetConnectionString("EntityModel")
    ?? "Server=(localdb)\\mssqllocaldb;Database=eShopCatalog;Trusted_Connection=True;MultipleActiveResultSets=true";

// Register EF Core DbContext
builder.Services.AddDbContext<EntityModel>(options =>
    options.UseSqlServer(connectionString));

// Register the WCF service with DI (scoped so each request gets a fresh context)
builder.Services.AddScoped<CatalogService>();

// Add CoreWCF services
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();

var app = builder.Build();

// Ensure database exists and seed initial data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EntityModel>();
    try
    {
        db.Database.EnsureCreated();
        CatalogDBInitializer.Seed(db);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Database initialization failed - service will start without seeded data.");
    }
}

// Configure CoreWCF endpoints
app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<CatalogService>(options =>
    {
        options.DebugBehavior.IncludeExceptionDetailInFaults = app.Environment.IsDevelopment();
    });

    var binding = new BasicHttpBinding
    {
        MaxReceivedMessageSize = int.MaxValue
    };

    serviceBuilder.AddServiceEndpoint<CatalogService, ICatalogService>(
        binding,
        "/CatalogService.svc");

    serviceBuilder.ConfigureServiceHostBase<CatalogService>(host =>
    {
        var smb = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
        if (smb != null)
            smb.HttpGetEnabled = true;
    });
});

app.Run();
