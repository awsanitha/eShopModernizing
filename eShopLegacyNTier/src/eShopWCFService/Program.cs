using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using eShopWCFService;
using eShopWCFService.Models.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register EF Core DbContext
builder.Services.AddDbContext<EntityModel>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("EntityModel")
        ?? "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=eShopDatabase;Persist Security Info=True;"));

// Register the WCF service
builder.Services.AddTransient<CatalogService>();

// Register CoreWCF services
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();

var app = builder.Build();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EntityModel>();
    context.Database.EnsureCreated();
    CatalogDBInitializer.Seed(context);
}

// Configure CoreWCF service endpoints
app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<CatalogService>(options =>
    {
        options.DebugBehavior.IncludeExceptionDetailInFaults = false;
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
