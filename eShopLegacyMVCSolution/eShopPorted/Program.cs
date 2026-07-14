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
using System;

namespace eShopPorted
{
    public class Program
    {
        public static DateTime StartTime { get; } = DateTime.UtcNow;

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Use Autofac as the DI container
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

            // Add ASP.NET Core MVC services
            builder.Services.AddControllersWithViews();

            bool useMockData = builder.Configuration.GetValue<bool>("UseMockData");
            if (!useMockData)
            {
                string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? "Server=(localdb)\\mssqllocaldb;Database=eShopPorted;Trusted_Connection=True;";

                builder.Services.AddDbContext<CatalogDBContext>(options =>
                    options.UseSqlServer(connectionString));
            }

            // Configure Autofac container
            builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                containerBuilder.RegisterModule(new ApplicationModule(useMockData));
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Catalog}/{action=Index}/{id?}");

            app.MapControllers();

            app.Run();
        }
    }
}
