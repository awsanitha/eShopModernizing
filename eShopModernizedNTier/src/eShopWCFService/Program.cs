using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using eShopWCFService;
using eShopWCFService.Models.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core DbContext
var connectionString = builder.Configuration.GetConnectionString("EntityModel")
    ?? "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=eShopDatabase;Persist Security Info=True;";
builder.Services.AddDbContext<EntityModel>(options =>
    options.UseSqlServer(connectionString));

// CoreWCF services
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddTransient<CatalogService>();

var app = builder.Build();

// Initialize / seed the database on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EntityModel>();
    CatalogDBInitializer.Initialize(context);
}

// Register the WCF service endpoint (replaces IIS / .svc hosting)
app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<CatalogService>(o =>
    {
        o.DebugBehavior.IncludeExceptionDetailInFaults = false;
    });

    serviceBuilder.AddServiceEndpoint<CatalogService, ICatalogService>(
        new BasicHttpBinding(),
        "/CatalogService.svc");

    serviceBuilder.ConfigureServiceHostBase<CatalogService>(host =>
    {
        var smb = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
        if (smb == null)
        {
            smb = new ServiceMetadataBehavior { HttpGetEnabled = true };
            host.Description.Behaviors.Add(smb);
        }
        else
        {
            smb.HttpGetEnabled = true;
        }
    });
});

app.Run();
