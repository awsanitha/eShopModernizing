using eShopWCFService.Models.Infrastructure;
using System.Linq;

namespace eShopWCFService.Models.Infrastructure
{
    /// <summary>
    /// Responsible for seeding the database with preconfigured data if it is empty.
    /// Called once at application startup after EnsureCreated().
    /// Replaces the EF6 CreateDatabaseIfNotExists initializer pattern.
    /// </summary>
    public static class CatalogDBInitializer
    {
        public static void Seed(eShopWCFService.EntityModel context)
        {
            // Only seed if the database is empty
            if (context.CatalogTypes.Any())
                return;

            AddCatalogTypes(context);
            AddCatalogBrands(context);
            AddCatalogItems(context);
            AddCatalogItemsStock(context);
            AddDiscountItems(context);
        }

        private static void AddCatalogTypes(eShopWCFService.EntityModel context)
        {
            var preconfiguredTypes = PreconfiguredData.GetPreconfiguredCatalogTypes();

            foreach (var type in preconfiguredTypes)
            {
                context.CatalogTypes.Add(type);
            }

            context.SaveChanges();
        }

        private static void AddCatalogBrands(eShopWCFService.EntityModel context)
        {
            var preconfiguredBrands = PreconfiguredData.GetPreconfiguredCatalogBrands();

            foreach (var brand in preconfiguredBrands)
            {
                context.CatalogBrands.Add(brand);
            }

            context.SaveChanges();
        }

        private static void AddDiscountItems(eShopWCFService.EntityModel context)
        {
            var preconfiguredDiscounts = PreconfiguredData.GetPreconfiguredDiscountItems();

            foreach (var discount in preconfiguredDiscounts)
            {
                context.DiscountItems.Add(discount);
            }

            context.SaveChanges();
        }

        private static void AddCatalogItems(eShopWCFService.EntityModel context)
        {
            var preconfiguredItems = PreconfiguredData.GetPreconfiguredCatalogItems();

            foreach (var item in preconfiguredItems)
            {
                context.CatalogItems.Add(item);
            }

            context.SaveChanges();
        }

        private static void AddCatalogItemsStock(eShopWCFService.EntityModel context)
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
