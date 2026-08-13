using eShopWCFService.Models;

namespace eShopWCFService.Models.Infrastructure
{
    public class CatalogDBInitializer
    {
        public static void SeedData(EntityModel context)
        {
            if (!context.CatalogTypes.Any())
            {
                context.CatalogTypes.AddRange(PreconfiguredData.GetPreconfiguredCatalogTypes());
                context.SaveChanges();
            }

            if (!context.CatalogBrands.Any())
            {
                context.CatalogBrands.AddRange(PreconfiguredData.GetPreconfiguredCatalogBrands());
                context.SaveChanges();
            }

            if (!context.CatalogItems.Any())
            {
                context.CatalogItems.AddRange(PreconfiguredData.GetPreconfiguredCatalogItems());
                context.SaveChanges();
            }

            if (!context.CatalogItemsStocks.Any())
            {
                context.CatalogItemsStocks.AddRange(PreconfiguredData.GetPreconfiguredCatalogItemsStock());
                context.SaveChanges();
            }

            if (!context.DiscountItems.Any())
            {
                context.DiscountItems.AddRange(PreconfiguredData.GetPreconfiguredDiscountItems());
                context.SaveChanges();
            }
        }
    }
}
