using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.Modules;
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

namespace eShopLegacyMVC
{
    public class Startup
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Startup));

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            var logRepository = LogManager.GetRepository(Assembly.GetExecutingAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo(Path.Combine(AppContext.BaseDirectory, "log4Net.xml")));
        }

        public static DateTime StartTime { get; } = DateTime.UtcNow;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            bool useMockData = Configuration.GetValue<bool>("UseMockData");
            if (!useMockData)
            {
                string connectionString = Configuration.GetConnectionString("CatalogDBContext");

                services.AddDbContext<CatalogDBContext>(options =>
                    options.UseSqlServer(connectionString));
            }
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            bool useMockData = Configuration.GetValue<bool>("UseMockData");
            builder.RegisterModule(new ApplicationModule(useMockData));
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

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Catalog}/{action=Index}/{id?}");
                endpoints.MapControllers();
            });

            ConfigDataBase(app);
        }

        private void ConfigDataBase(IApplicationBuilder app)
        {
            bool useMockData = Configuration.GetValue<bool>("UseMockData");

            if (!useMockData)
            {
                using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<CatalogDBContext>();
                    var initializer = scope.ServiceProvider.GetRequiredService<CatalogDBInitializer>();
                    initializer.Initialize(context);
                }
            }
        }
    }
}
