using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.Modules;
using log4net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Reflection;

namespace eShopLegacyMVC
{
    public class Startup
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // Configure log4net
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo("log4Net.xml"));
        }

        public IConfiguration Configuration { get; }

        // Returns IServiceProvider to integrate Autofac with ASP.NET Core DI
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options => options.EnableEndpointRouting = false);
            services.AddDistributedMemoryCache();
            services.AddSession();
            services.AddHttpContextAccessor();

            bool useMockData = Configuration.GetValue<bool>("UseMockData");
            if (!useMockData)
            {
                string connectionString = Configuration.GetConnectionString("CatalogDBContext");
                services.AddDbContext<CatalogDBContext>(options =>
                    options.UseSqlServer(connectionString));
            }

            // Create Autofac container builder
            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.RegisterModule(new ApplicationModule(useMockData));

            ILifetimeScope container = builder.Build();

            return new AutofacServiceProvider(container);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseSession();

            // Initialize session values for new sessions
            app.Use(async (context, next) =>
            {
                if (context.Session.GetString("MachineName") == null)
                {
                    context.Session.SetString("MachineName", System.Environment.MachineName);
                    context.Session.SetString("SessionStartTime", DateTime.Now.ToString("g"));
                }
                await next();
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "GetPicRouteTemplate",
                    template: "items/{catalogItemId:int}/pic",
                    defaults: new { controller = "Pic", action = "Index" });

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Catalog}/{action=Index}/{id?}");
            });

            // Initialize the database (non-mock mode)
            bool useMockData = Configuration.GetValue<bool>("UseMockData");
            if (!useMockData)
            {
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    try
                    {
                        var db = scope.ServiceProvider.GetService<CatalogDBContext>();
                        var initializer = scope.ServiceProvider.GetService<CatalogDBInitializer>();
                        initializer?.Seed(db);
                    }
                    catch (Exception ex)
                    {
                        _log.Error("An error occurred while seeding the database.", ex);
                    }
                }
            }
        }
    }
}
