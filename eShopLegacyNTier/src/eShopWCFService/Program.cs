using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using eShopWCFService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceModelServices()
    .AddServiceModelMetadata();

builder.Services.AddTransient<CatalogService>();

var app = builder.Build();

app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<CatalogService>(options =>
    {
        options.DebugBehavior.IncludeExceptionDetailInFaults = app.Environment.IsDevelopment();
    });

    serviceBuilder.AddServiceEndpoint<CatalogService, ICatalogService>(
        new BasicHttpBinding(),
        "/CatalogService.svc");

    serviceBuilder.ConfigureServiceHostBase<CatalogService>(host =>
    {
        var metadataBehavior = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
        if (metadataBehavior != null)
            metadataBehavior.HttpGetEnabled = true;
    });
});

app.Run();
