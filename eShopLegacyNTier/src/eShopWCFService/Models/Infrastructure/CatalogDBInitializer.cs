using eShopWCFService.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eShopWCFService.Models.Infrastructure
{
    /// <summary>
    /// Seeds the catalog database with preconfigured data if the tables are empty.
    /// Called from Program.cs after EnsureCreated().
    /// Replaces the EF6 <c>CreateDatabaseIfNotExists&lt;EntityModel&gt;</c> initializer.
    /// </summary>
    public static class CatalogDBInitializer
    {
        public static void Seed(EntityModel context)
        {
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
