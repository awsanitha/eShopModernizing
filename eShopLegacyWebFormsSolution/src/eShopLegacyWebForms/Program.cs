using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Models.Infrastructure;
using eShopLegacyWebForms.Modules;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Reflection;

namespace eShopLegacyWebForms
{
    public class Program
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Program));

        public static void Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4Net.xml"));

            var builder = WebApplication.CreateBuilder(args);

            bool useMockData = builder.Configuration.GetValue<bool>("UseMockData");
            bool useCustomizationData = builder.Configuration.GetValue<bool>("UseCustomizationData");

            builder.Services.AddControllersWithViews();

            if (!useMockData)
            {
                string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                builder.Services.AddDbContext<CatalogDBContext>(options =>
                    options.UseSqlServer(connectionString));
            }

            // Configure Autofac as the DI container.
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                containerBuilder.RegisterModule(new ApplicationModule(useMockData));
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.MapControllerRoute(
                name: "ProductsByPageRoute",
                pattern: "Default/index/{index}/size/{size}",
                defaults: new { controller = "Catalog", action = "Index" });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Catalog}/{action=Index}/{id?}");

            if (!useMockData)
            {
                using (var scope = app.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<CatalogDBContext>();
                    var indexGenerator = scope.ServiceProvider.GetRequiredService<CatalogItemHiLoGenerator>();
                    var initializer = new CatalogDBInitializer(indexGenerator, useCustomizationData, app.Environment.ContentRootPath);
                    initializer.Initialize(context);
                }
            }

            app.Run();
        }
    }
}
