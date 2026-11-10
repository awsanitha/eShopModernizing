using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using eShopWCFService;
using eShopWCFService.Models.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EntityModel>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EntityModel")));

builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddTransient<CatalogService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EntityModel>();
    CatalogDBInitializer.Initialize(context);
}

app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<CatalogService>(serviceOptions =>
    {
        serviceOptions.DebugBehavior.IncludeExceptionDetailInFaults = true;
    });
    serviceBuilder.AddServiceEndpoint<CatalogService, ICatalogService>(
        new BasicHttpBinding(), "/CatalogService.svc");
});

var mb = app.Services.GetRequiredService<ServiceMetadataBehavior>();
mb.HttpGetEnabled = true;

app.Run();
