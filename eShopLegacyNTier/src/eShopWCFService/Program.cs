using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using eShopWCFService;
using eShopWCFService.Models.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Connection string: env override takes precedence over appsettings.json ──
string connectionString =
    CatalogConfiguration.EnvironmentOverride
    ?? builder.Configuration.GetConnectionString(CatalogConfiguration.ConnectionStringName)
    ?? throw new InvalidOperationException(
        $"Connection string '{CatalogConfiguration.ConnectionStringName}' not found.");

// ── EF Core ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<EntityModel>(options =>
    options.UseSqlServer(connectionString));

// ── CatalogService (scoped = one instance per WCF call) ──────────────────────
builder.Services.AddScoped<CatalogService>();

// ── CoreWCF ──────────────────────────────────────────────────────────────────
builder.Services
    .AddServiceModelServices()
    .AddServiceModelMetadata();

var app = builder.Build();

// ── Ensure database exists and is seeded ──────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EntityModel>();
    db.Database.EnsureCreated();
    CatalogDBInitializer.Seed(db);
}

// ── CoreWCF endpoint registration ─────────────────────────────────────────────
app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<CatalogService>(o =>
    {
        o.DebugBehavior.IncludeExceptionDetailInFaults =
            app.Environment.IsDevelopment();
    });

    // Expose the service at the same URL path as the legacy .svc file
    serviceBuilder.AddServiceEndpoint<CatalogService, ICatalogService>(
        new BasicHttpBinding(),
        "/CatalogService.svc");

    serviceBuilder.ConfigureServiceHostBase<CatalogService>(host =>
    {
        var metadataBehavior =
            host.Description.Behaviors.Find<ServiceMetadataBehavior>();
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
