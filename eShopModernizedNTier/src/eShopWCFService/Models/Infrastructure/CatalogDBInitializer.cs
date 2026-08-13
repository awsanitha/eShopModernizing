using eShopWCFService;
using eShopWCFService.Models.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eShopWCFService.Models.Infrastructure
{
    /// <summary>
    /// Seeds the database with preconfigured data if it is empty.
    /// Called from Program.cs after EnsureCreated().
    /// </summary>
    public static class CatalogDBInitializer
    {
        public static void Seed(EntityModel context)
        {
            AddCatalogTypes(context);
            AddCatalogBrands(context);
            AddCatalogItems(context);
            AddCatalogItemsStock(context);
            AddDiscountItems(context);
        }

        private static void AddCatalogTypes(EntityModel context)
        {
            if (context.CatalogTypes.Any())
                return;

            var preconfiguredTypes = PreconfiguredData.GetPreconfiguredCatalogTypes();
            foreach (var type in preconfiguredTypes)
            {
                context.CatalogTypes.Add(type);
            }
            context.SaveChanges();
        }

        private static void AddCatalogBrands(EntityModel context)
        {
            if (context.CatalogBrands.Any())
                return;

            var preconfiguredBrands = PreconfiguredData.GetPreconfiguredCatalogBrands();
            foreach (var brand in preconfiguredBrands)
            {
                context.CatalogBrands.Add(brand);
            }
            context.SaveChanges();
        }

        private static void AddDiscountItems(EntityModel context)
        {
            if (context.DiscountItems.Any())
                return;

            var preconfiguredDiscounts = PreconfiguredData.GetPreconfiguredDiscountItems();
            foreach (var discount in preconfiguredDiscounts)
            {
                context.DiscountItems.Add(discount);
            }
            context.SaveChanges();
        }

        private static void AddCatalogItems(EntityModel context)
        {
            if (context.CatalogItems.Any())
                return;

            var preconfiguredItems = PreconfiguredData.GetPreconfiguredCatalogItems();
            foreach (var item in preconfiguredItems)
            {
                context.CatalogItems.Add(item);
            }
            context.SaveChanges();
        }

        private static void AddCatalogItemsStock(EntityModel context)
        {
            if (context.CatalogItemsStocks.Any())
                return;

            var preconfiguredStock = PreconfiguredData.GetPreconfiguredCatalogItemsStock();
            foreach (var s in preconfiguredStock)
            {
                context.CatalogItemsStocks.Add(s);
            }
            context.SaveChanges();
        }
    }
}
