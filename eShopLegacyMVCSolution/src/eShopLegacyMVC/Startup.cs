using Autofac;
using eShopLegacyMVC.Controllers;
using eShopLegacyMVC.Models;
using eShopLegacyMVC.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace eShopLegacyMVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddSession();

            bool useMockData = Configuration.GetValue<bool>("UseMockData");
            if (!useMockData)
            {
                string connectionString = Configuration.GetConnectionString("CatalogDBContext")
                    ?? "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Microsoft.eShopOnContainers.Services.CatalogDb;Integrated Security=True;MultipleActiveResultSets=True;";

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
                app.UseExceptionHandler("/Catalog/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseSession();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: PicController.GetPicRouteName,
                    pattern: "items/{catalogItemId:int}/pic",
                    defaults: new { controller = "Pic", action = "Index" });
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Catalog}/{action=Index}/{id?}");
            });
        }
    }
}
