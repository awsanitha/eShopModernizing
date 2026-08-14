using eShopWCFService;
using System.Linq;

namespace eShopWCFService.Models.Infrastructure
{
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
