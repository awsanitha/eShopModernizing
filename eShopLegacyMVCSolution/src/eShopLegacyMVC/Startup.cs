using Autofac;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

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

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            bool useMockData = Configuration.GetValue<bool>("UseMockData");
            if (!useMockData)
            {
                string connectionString = Configuration.GetConnectionString("DefaultConnection")!;
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
        }
    }
}
