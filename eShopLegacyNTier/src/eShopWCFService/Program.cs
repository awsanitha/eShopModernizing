using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using eShopWCFService;
using eShopWCFService.Models.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure EF Core DbContext
builder.Services.AddDbContext<EntityModel>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("EntityModel")
        ?? "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=eShopDatabase;Persist Security Info=True;"));

// Register CoreWCF services
builder.Services.AddServiceModelServices()
                .AddServiceModelMetadata();

// Register the catalog service
builder.Services.AddTransient<CatalogService>();

var app = builder.Build();

// Ensure the database is created and seeded on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EntityModel>();
    db.Database.EnsureCreated();
    CatalogDBSeeder.Seed(db);
}

// Configure CoreWCF service model
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
