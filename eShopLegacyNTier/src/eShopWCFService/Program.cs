using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using Microsoft.EntityFrameworkCore;
using eShopWCFService;
using eShopWCFService.Models.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Register EF Core DbContext
builder.Services.AddDbContext<EntityModel>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EntityModel")
        ?? CatalogConfiguration.ConnectionString));

// Register the WCF service with CoreWCF
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddTransient<CatalogService>();

var app = builder.Build();

// Seed database on first run
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EntityModel>();
    CatalogDBInitializer.Initialize(db);
}

// Configure CoreWCF service endpoints
app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<CatalogService>(serviceOptions =>
    {
        serviceOptions.DebugBehavior.IncludeExceptionDetailInFaults = true;
    });

    // BasicHttpBinding endpoint – mirrors the original WCF configuration
    serviceBuilder.AddServiceEndpoint<CatalogService, ICatalogService>(
        new BasicHttpBinding(), "/CatalogService.svc");

    // Metadata endpoint (replaces serviceMetadata httpGetEnabled="true")
    var metadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
    metadataBehavior.HttpGetEnabled = true;
});

app.Run();
