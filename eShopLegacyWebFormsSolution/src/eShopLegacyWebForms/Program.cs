using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Models.Infrastructure;
using eShopLegacyWebForms.Modules;
using log4net;
using log4net.Config;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.xml")]

var log4netRepository = LogManager.GetRepository(Assembly.GetEntryAssembly()!);
XmlConfigurator.Configure(log4netRepository, new FileInfo("log4Net.xml"));

var builder = WebApplication.CreateBuilder(args);

// Use Autofac as DI container
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Add Razor Pages
builder.Services.AddRazorPages();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Configure EF Core if not using mock data
var useMockData = bool.Parse(builder.Configuration["AppSettings:UseMockData"] ?? "true");
if (!useMockData)
{
    builder.Services.AddDbContext<CatalogDBContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogDBContext")));
}

// Configure Autofac container
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new ApplicationModule(useMockData));
});

// Add logging (log4net integration)
builder.Logging.AddLog4Net("log4Net.xml");

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.MapRazorPages();

// Initialize database on startup (if not using mock data)
if (!useMockData)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDBContext>();
    var indexGenerator = scope.ServiceProvider.GetRequiredService<CatalogItemHiLoGenerator>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    var initializer = new CatalogDBInitializer(indexGenerator, configuration, environment);
    initializer.Initialize(dbContext);
}

app.Run();
