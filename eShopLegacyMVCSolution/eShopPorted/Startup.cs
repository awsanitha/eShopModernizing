using Autofac;
using eShopPorted.Models;
using eShopPorted.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace eShopPorted
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static DateTime StartTime { get; } = DateTime.UtcNow;
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            bool useMockData = Configuration.GetValue<bool>("UseMockData");
            if (!useMockData)
            {
                string connectionString = Configuration.GetConnectionString("DefaultConnection")
                    ?? "Data Source=(localdb)\\MSSQLLocalDB;Database=eShopPorted;Trusted_Connection=True;MultipleActiveResultSets=true";

                services.AddDbContext<CatalogDBContext>(options =>
                    options.UseSqlServer(connectionString));
            }
        }

        // Configure Autofac container
        public void ConfigureContainer(ContainerBuilder builder)
        {
            bool useMockData = Configuration.GetValue<bool>("UseMockData");
            builder.RegisterModule(new ApplicationModule(useMockData));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
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
