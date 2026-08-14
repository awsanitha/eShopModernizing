using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using eShopWCFService;
using eShopWCFService.Models.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ----- EF Core -----
var connectionString = CatalogConfiguration.EnvironmentConnectionString
    ?? builder.Configuration.GetConnectionString(CatalogConfiguration.ConnectionStringKey);

builder.Services.AddDbContext<EntityModel>(options =>
    options.UseSqlServer(connectionString));

// Register CatalogService for DI (CoreWCF uses DI to create service instances)
builder.Services.AddScoped<CatalogService>();

// ----- CoreWCF -----
builder.Services.AddServiceModelServices()
    .AddServiceModelMetadata();

var app = builder.Build();

// ----- Database initialization -----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EntityModel>();
    db.Database.EnsureCreated();
    CatalogDBInitializer.Seed(db);
}

// ----- CoreWCF endpoint wiring -----
app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<CatalogService>(serviceOptions =>
    {
        serviceOptions.DebugBehavior.IncludeExceptionDetailInFaults =
            app.Environment.IsDevelopment();
    });

    serviceBuilder.AddServiceEndpoint<CatalogService, ICatalogService>(
        new BasicHttpBinding(),
        "/CatalogService.svc");

    var serviceMetadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
    serviceMetadataBehavior.HttpGetEnabled = true;
});

app.Run();
