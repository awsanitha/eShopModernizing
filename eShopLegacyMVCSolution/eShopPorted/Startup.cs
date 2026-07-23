using eShopPorted.Models;
using eShopPorted.Services;
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

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            bool useMockData = Configuration.GetValue<bool>("UseMockData");
            if (!useMockData)
            {
                string connectionString = Configuration.GetConnectionString("DefaultConnection");
                services.AddDbContext<CatalogDBContext>(options =>
                    options.UseSqlServer(connectionString));
                services.AddScoped<ICatalogService, CatalogService>();
            }
            else
            {
                services.AddSingleton<ICatalogService, CatalogServiceMock>();
            }
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
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
