using eShopWCFService.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace eShopWCFService.Models.Infrastructure
{
    /// <summary>
    /// Initializes the database schema and seeds preconfigured data if the database is empty.
    /// Replaces the EF6 CreateDatabaseIfNotExists initializer pattern.
    /// </summary>
    public static class CatalogDBInitializer
    {
        public static void Initialize(EntityModel context)
        {
            // Ensure the database and schema are created
            context.Database.EnsureCreated();

            // Only seed data if the catalog is empty
            if (context.CatalogTypes.Any())
                return;

            AddCatalogTypes(context);
            AddCatalogBrands(context);
            AddCatalogItems(context);
            AddCatalogItemsStock(context);
            AddDiscountItems(context);
        }

        private static void AddCatalogTypes(EntityModel context)
        {
            var preconfiguredTypes = PreconfiguredData.GetPreconfiguredCatalogTypes();
            foreach (var type in preconfiguredTypes)
            {
                context.CatalogTypes.Add(type);
            }
            context.SaveChanges();
        }

        private static void AddCatalogBrands(EntityModel context)
        {
            var preconfiguredBrands = PreconfiguredData.GetPreconfiguredCatalogBrands();
            foreach (var brand in preconfiguredBrands)
            {
                context.CatalogBrands.Add(brand);
            }
            context.SaveChanges();
        }

        private static void AddDiscountItems(EntityModel context)
        {
            var preconfiguredDiscounts = PreconfiguredData.GetPreconfiguredDiscountItems();
            foreach (var discount in preconfiguredDiscounts)
            {
                context.DiscountItems.Add(discount);
            }
            context.SaveChanges();
        }

        private static void AddCatalogItems(EntityModel context)
        {
            var preconfiguredItems = PreconfiguredData.GetPreconfiguredCatalogItems();
            foreach (var item in preconfiguredItems)
            {
                context.CatalogItems.Add(item);
            }
            context.SaveChanges();
        }

        private static void AddCatalogItemsStock(EntityModel context)
        {
            var preconfiguredStock = PreconfiguredData.GetPreconfiguredCatalogItemsStock();
            foreach (var s in preconfiguredStock)
            {
                context.CatalogItemsStocks.Add(s);
            }
            context.SaveChanges();
        }
    }
}
