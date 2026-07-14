using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace eShopLegacyMVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure log4net
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4Net.xml"));

            // Use Autofac as the DI container
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

            // Add ASP.NET Core MVC services
            builder.Services.AddControllersWithViews();

            // Read config values
            bool useMockData = builder.Configuration.GetValue<bool>("UseMockData");

            if (!useMockData)
            {
                string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? "Server=(localdb)\\mssqllocaldb;Database=eShopLegacyMVC;Trusted_Connection=True;";

                builder.Services.AddDbContext<CatalogDBContext>(options =>
                    options.UseSqlServer(connectionString));
            }

            // Configure Autofac container
            builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                containerBuilder.RegisterModule(new ApplicationModule(useMockData));

                if (!useMockData)
                {
                    containerBuilder.RegisterType<CatalogDBInitializer>()
                        .InstancePerLifetimeScope();

                    containerBuilder.RegisterType<CatalogItemHiLoGenerator>()
                        .SingleInstance();
                }
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Catalog/Error");
                app.UseHsts();
            }

            // Serve static files from the project content root (Content/, Scripts/, Images/, Pics/, fonts/)
            app.UseStaticFiles(new Microsoft.AspNetCore.Builder.StaticFileOptions
            {
                FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
                    app.Environment.ContentRootPath),
                RequestPath = ""
            });

            app.UseRouting();

            app.MapControllerRoute(
                name: "GetPicRouteTemplate",
                pattern: "items/{catalogItemId:int}/pic",
                defaults: new { controller = "Pic", action = "Index" });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Catalog}/{action=Index}/{id?}");

            app.MapControllers();

            // Initialize the database if not using mock data
            if (!useMockData)
            {
                using var scope = app.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetService<CatalogDBContext>();
                if (dbContext != null)
                {
                    dbContext.Database.EnsureCreated();
                    var initializer = scope.ServiceProvider.GetService<CatalogDBInitializer>();
                    initializer?.Seed(dbContext);
                }
            }

            app.Run();
        }
    }
}
