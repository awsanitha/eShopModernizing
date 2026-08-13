using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace eShopWCFService.Models.Infrastructure
{
    /// <summary>
    /// Replaces EF6 CatalogDBInitializer / CreateDatabaseIfNotExists pattern.
    /// Call Initialize() once at application startup (from Program.cs).
    /// </summary>
    public static class CatalogDBInitializer
    {
        public static void Initialize(EntityModel context)
        {
            // Create the database schema if it does not exist yet.
            context.Database.EnsureCreated();

            // If data already exists, skip seeding to avoid duplicates.
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
