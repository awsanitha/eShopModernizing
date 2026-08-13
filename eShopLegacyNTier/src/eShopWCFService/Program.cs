using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using eShopWCFService;
using eShopWCFService.Models.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Connection string: prefer environment variable override, then appsettings.json
var connectionString = Environment.GetEnvironmentVariable("ConnectionString")
    ?? builder.Configuration.GetConnectionString("EntityModel")
    ?? "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=eShopDatabase;Persist Security Info=True;";

// Register EF Core DbContext
builder.Services.AddDbContext<EntityModel>(options =>
    options.UseSqlServer(connectionString));

// Register the WCF service with scoped lifetime to match the DbContext scope
builder.Services.AddScoped<CatalogService>();

// Add CoreWCF services
builder.Services.AddServiceModelServices()
                .AddServiceModelMetadata();

var app = builder.Build();

// Ensure database is created and seeded on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<EntityModel>();
    dbContext.Database.EnsureCreated();
    CatalogDBInitializer.Seed(dbContext);
}

// Configure CoreWCF endpoints
app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<CatalogService>(serviceOptions =>
    {
        serviceOptions.DebugBehavior.IncludeExceptionDetailInFaults = false;
    });

    serviceBuilder.AddServiceEndpoint<CatalogService, ICatalogService>(
        new BasicHttpBinding(),
        "/CatalogService.svc");

    serviceBuilder.ConfigureServiceHostBase<CatalogService>(host =>
    {
        var metadataBehavior = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
        if (metadataBehavior != null)
        {
            metadataBehavior.HttpGetEnabled = true;
        }
    });
});

app.Run();
