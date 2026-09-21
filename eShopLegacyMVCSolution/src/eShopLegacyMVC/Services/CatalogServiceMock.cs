using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.ViewModel;
using log4net;
using System.Reflection;

namespace eShopLegacyMVC.Services
{
    public class CatalogServiceMock : ICatalogService
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private List<CatalogItem> catalogItems;

        public CatalogServiceMock()
        {
            catalogItems = PreconfiguredData.GetPreconfiguredCatalogItems();
        }

        public PaginatedItemsViewModel<CatalogItem> GetCatalogItemsPaginated(int pageSize, int pageIndex)
        {
            _log.Debug($"GetCatalogItemsPaginated called pageSize={pageSize} pageIndex={pageIndex}");

            var totalItems = catalogItems.LongCount();
            var itemsOnPage = ComposeCatalogItems(catalogItems)
                .OrderBy(c => c.Id)
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToList();

            return new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);
        }

        public CatalogItem? FindCatalogItem(int id)
        {
            var item = catalogItems.FirstOrDefault(x => x.Id == id);
            if (item == null) return null;
            return ComposeCatalogItems(new List<CatalogItem> { item }).First();
        }

        public IEnumerable<CatalogBrand> GetCatalogBrands()
        {
            return PreconfiguredData.GetPreconfiguredCatalogBrands();
        }

        public IEnumerable<CatalogType> GetCatalogTypes()
        {
            return PreconfiguredData.GetPreconfiguredCatalogTypes();
        }

        public void CreateCatalogItem(CatalogItem catalogItem)
        {
            var id = catalogItems.Any() ? catalogItems.Max(i => i.Id) + 1 : 1;
            catalogItem.Id = id;
            catalogItems.Add(catalogItem);
        }

        public void UpdateCatalogItem(CatalogItem catalogItem)
        {
            var index = catalogItems.FindIndex(i => i.Id == catalogItem.Id);
            if (index >= 0)
                catalogItems[index] = catalogItem;
        }

        public void RemoveCatalogItem(CatalogItem catalogItem)
        {
            catalogItems.Remove(catalogItem);
        }

        public void Dispose()
        {
        }

        private IEnumerable<CatalogItem> ComposeCatalogItems(IEnumerable<CatalogItem> items)
        {
            var brands = PreconfiguredData.GetPreconfiguredCatalogBrands().ToList();
            var types = PreconfiguredData.GetPreconfiguredCatalogTypes().ToList();

            foreach (var item in items)
            {
                item.CatalogBrand = brands.FirstOrDefault(b => b.Id == item.CatalogBrandId);
                item.CatalogType = types.FirstOrDefault(t => t.Id == item.CatalogTypeId);
                yield return item;
            }
        }
    }
}
