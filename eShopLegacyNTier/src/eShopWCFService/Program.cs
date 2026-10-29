using System;
using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using eShopWCFService;
using eShopWCFService.Models.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Bridges the legacy ConfigurationManager-based connection string lookup to appsettings.json.
CatalogConfiguration.Initialize(builder.Configuration);

builder.Services.AddDbContext<EntityModel>(options =>
    options.UseSqlServer(CatalogConfiguration.ConnectionString));

builder.Services.AddScoped<CatalogService>();

builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();

var app = builder.Build();

// The EF6 CreateDatabaseIfNotExists initializer ran on first database access; it is now
// explicit. A database that is unreachable at startup is reported but does not stop the
// service from starting, matching the previous lazy behaviour.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<EntityModel>();
        new CatalogDBInitializer().InitializeDatabase(context);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Could not create or seed the catalog database at startup.");
    }
}

var metadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
metadataBehavior.HttpGetEnabled = true;
metadataBehavior.HttpsGetEnabled = true;

app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<CatalogService>(serviceOptions =>
    {
        serviceOptions.DebugBehavior.IncludeExceptionDetailInFaults = false;
    });

    serviceBuilder.AddServiceEndpoint<CatalogService, ICatalogService>(
        new BasicHttpBinding(), "/CatalogService.svc");
});

app.Run();

/// <summary>
/// Named entry-point type so the startup logger category matches the host.
/// </summary>
public partial class Program
{
}
