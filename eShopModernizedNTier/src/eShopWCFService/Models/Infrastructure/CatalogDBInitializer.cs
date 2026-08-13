using eShopWCFService.Models;
using System;
using System.Linq;

namespace eShopWCFService.Models.Infrastructure
{
    /// <summary>
    /// EF Core-compatible database initializer.
    /// Replaces the EF6 CreateDatabaseIfNotExists&lt;T&gt; initializer pattern.
    /// Call Initialize() from Program.cs at startup after DI is resolved.
    /// </summary>
    public static class CatalogDBInitializer
    {
        public static void Initialize(EntityModel context)
        {
            // Creates the database if it doesn't exist (equivalent to CreateDatabaseIfNotExists)
            context.Database.EnsureCreated();

            // Seed data only if the tables are empty
            if (!context.CatalogTypes.Any())
                AddCatalogTypes(context);

            if (!context.CatalogBrands.Any())
                AddCatalogBrands(context);

            if (!context.CatalogItems.Any())
                AddCatalogItems(context);

            if (!context.CatalogItemsStocks.Any())
                AddCatalogItemsStock(context);

            if (!context.DiscountItems.Any())
                AddDiscountItems(context);
        }

        private static void AddCatalogTypes(EntityModel context)
        {
            foreach (var type in PreconfiguredData.GetPreconfiguredCatalogTypes())
                context.CatalogTypes.Add(type);
            context.SaveChanges();
        }

        private static void AddCatalogBrands(EntityModel context)
        {
            foreach (var brand in PreconfiguredData.GetPreconfiguredCatalogBrands())
                context.CatalogBrands.Add(brand);
            context.SaveChanges();
        }

        private static void AddDiscountItems(EntityModel context)
        {
            foreach (var discount in PreconfiguredData.GetPreconfiguredDiscountItems())
                context.DiscountItems.Add(discount);
            context.SaveChanges();
        }

        private static void AddCatalogItems(EntityModel context)
        {
            foreach (var item in PreconfiguredData.GetPreconfiguredCatalogItems())
                context.CatalogItems.Add(item);
            context.SaveChanges();
        }

        private static void AddCatalogItemsStock(EntityModel context)
        {
            foreach (var s in PreconfiguredData.GetPreconfiguredCatalogItemsStock())
                context.CatalogItemsStocks.Add(s);
            context.SaveChanges();
        }
    }
}
