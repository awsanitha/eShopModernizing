using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopPorted.Models;
using eShopPorted.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace eShopPorted
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public static System.DateTime StartTime { get; } = System.DateTime.UtcNow;
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            bool useMockData = Configuration.GetValue<bool>("UseMockData");
            if (!useMockData)
            {
                string connectionString = Configuration.GetConnectionString("DefaultConnection")!;

                services.AddDbContext<CatalogDBContext>(options =>
                    options.UseSqlServer(connectionString)
                );
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
            });
        }
    }
}
