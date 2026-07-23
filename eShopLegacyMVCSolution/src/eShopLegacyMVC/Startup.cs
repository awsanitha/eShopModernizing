using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;

namespace eShopLegacyMVC
{
    public class Startup
    {
        public static DateTime StartTime { get; } = DateTime.UtcNow;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddSession();

            bool useMockData = bool.Parse(Configuration["UseMockData"] ?? "false");
            if (!useMockData)
            {
                string connectionString = Configuration.GetConnectionString("CatalogDBContext")
                    ?? Configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("No database connection string configured.");

                services.AddDbContext<CatalogDBContext>(options =>
                    options.UseSqlServer(connectionString));
            }

            // Configure Autofac container
            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.RegisterModule(new ApplicationModule(useMockData));

            ILifetimeScope container = builder.Build();
            return new AutofacServiceProvider(container);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Catalog/Error");
            }

            // Serve static files from wwwroot
            app.UseStaticFiles();

            // Serve static files from legacy Content/ directory
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(env.ContentRootPath, "Content")),
                RequestPath = "/Content"
            });

            // Serve static files from legacy Scripts/ directory
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(env.ContentRootPath, "Scripts")),
                RequestPath = "/Scripts"
            });

            // Serve static files from legacy Images/ directory
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(env.ContentRootPath, "Images")),
                RequestPath = "/images"
            });

            // Serve static files from legacy Pics/ directory
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(env.ContentRootPath, "Pics")),
                RequestPath = "/Pics"
            });

            // Serve static files from legacy fonts/ directory
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(env.ContentRootPath, "fonts")),
                RequestPath = "/fonts",
                ContentTypeProvider = new FileExtensionContentTypeProvider()
            });

            app.UseSession();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Catalog}/{action=Index}/{id?}");
            });

            // Seed database on startup
            bool useMockData = bool.Parse(Configuration["UseMockData"] ?? "false");
            if (!useMockData)
            {
                using var scope = app.ApplicationServices.CreateScope();
                var initializer = scope.ServiceProvider.GetService<CatalogDBInitializer>();
                var context = scope.ServiceProvider.GetService<CatalogDBContext>();
                if (initializer != null && context != null)
                {
                    initializer.Initialize(context);
                }
            }
        }
    }
}
