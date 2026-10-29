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

// Bridges the legacy Web.config connection string lookup to appsettings.json.
CatalogConfiguration.Initialize(builder.Configuration);

builder.Services.AddDbContext<EntityModel>(options =>
    options.UseSqlServer(
        CatalogConfiguration.ConnectionString,
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddScoped<CatalogService>();

builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();

var app = builder.Build();

// The EF6 CreateDatabaseIfNotExists initializer ran on first database access. This is the
// eager attempt; CatalogDBInitializer.EnsureInitialized only latches on success, so if the
// database is unreachable now, the first operation that reaches it retries.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var context = scope.ServiceProvider.GetRequiredService<EntityModel>();
    CatalogDBInitializer.EnsureInitialized(context, logger);
}

var metadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();

// Replaces <serviceMetadata httpGetEnabled="true"/>. This serves the WSDL at
// /CatalogService.svc?wsdl (and ?singleWsdl), which is how metadata is published on CoreWCF.
//
// The legacy Web.config also declared a WS-MetadataExchange endpoint:
//   <endpoint address="mex" binding="mexHttpBinding" contract="IMetadataExchange"/>
// CoreWCF 1.9.1 has no equivalent. ServiceMetadataBehavior exposes only the HTTP(S) GET
// metadata properties, and AddServiceEndpoint<CatalogService, IMetadataExchange>(...) is
// rejected at startup ("Service type ... doesn't implement interface IMetadataExchange")
// because CoreWCF requires the service type to implement the endpoint's contract and does not
// substitute its own mex implementation the way .NET Framework WCF did. Metadata discovery
// itself is preserved: svcutil / "Add Service Reference" consume the ?wsdl URL below.
metadataBehavior.HttpGetEnabled = true;

// Left disabled deliberately. The legacy Web.config set httpsGetEnabled="true", but this host
// listens on HTTP only (see Kestrel:Endpoints in appsettings.json) and the eShopWinForms client
// connects over http. CoreWCF, like WCF, throws at startup when HTTPS metadata is requested
// with no HTTPS base address. Enable this together with an Https Kestrel endpoint.
metadataBehavior.HttpsGetEnabled = false;

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
