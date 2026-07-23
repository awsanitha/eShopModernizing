using eShopWCFService.Models;
using eShopWCFService.Models.Infrastructure;
using System.Linq;

namespace eShopWCFService.Models.Infrastructure
{
    /// <summary>
    /// Provides data seeding for the catalog database on EF Core.
    /// Called once at application startup after EnsureCreated().
    /// </summary>
    public static class CatalogDBSeeder
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
