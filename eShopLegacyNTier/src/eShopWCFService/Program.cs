using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using eShopWCFService;
using eShopWCFService.Models.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add CoreWCF services
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();

// Add EF Core
builder.Services.AddDbContext<EntityModel>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("EntityModel")
        ?? "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=eShopDatabase;Persist Security Info=True;"));

// Register the WCF service with DI (scoped matches DbContext lifetime)
builder.Services.AddScoped<CatalogService>();

var app = builder.Build();

// Initialize the database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EntityModel>();
    db.Database.EnsureCreated();
    CatalogDBInitializer.SeedIfEmpty(db);
}

// Configure CoreWCF service endpoint
app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<CatalogService>();
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
