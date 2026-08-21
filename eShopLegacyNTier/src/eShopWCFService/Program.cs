using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using eShopWCFService;
using eShopWCFService.Models.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddTransient<CatalogService>();

var app = builder.Build();

// Initialize the database on startup (EnsureCreated + seed if empty)
try
{
    using var context = new EntityModel();
    CatalogDBInitializer.Initialize(context);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while initializing the database.");
}

app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<CatalogService>(o =>
    {
        o.DebugBehavior.IncludeExceptionDetailInFaults = true;
    });

    serviceBuilder.AddServiceEndpoint<CatalogService, ICatalogService>(
        new BasicHttpBinding(), "/CatalogService.svc");

    serviceBuilder.ConfigureServiceHostBase<CatalogService>(host =>
    {
        var metaBehavior = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
        if (metaBehavior != null)
        {
            metaBehavior.HttpGetEnabled = true;
        }
    });
});

app.Run();
