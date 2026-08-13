using eShopWCFService.Models;
using eShopWCFService.Models.Infrastructure;

namespace eShopWCFService
{
    public class CatalogServiceMock : ICatalogService
    {
        private List<CatalogItem> _catalogItems;
        private List<CatalogBrand> _catalogBrands;
        private List<CatalogType> _catalogTypes;
        private List<CatalogItemsStock> _catalogItemsStock;

        public CatalogServiceMock()
        {
            _catalogItems = new List<CatalogItem>(PreconfiguredData.GetPreconfiguredCatalogItems());
            _catalogBrands = new List<CatalogBrand>(PreconfiguredData.GetPreconfiguredCatalogBrands());
            _catalogTypes = new List<CatalogType>(PreconfiguredData.GetPreconfiguredCatalogTypes());
            _catalogItemsStock = new List<CatalogItemsStock>(PreconfiguredData.GetPreconfiguredCatalogItemsStock());
        }

        public CatalogItem FindCatalogItem(int id)
        {
            return _catalogItems.FirstOrDefault(x => x.Id == id)!;
        }

        public List<CatalogItem> GetCatalogItems(int brandIdFilter, int typeIdFilter)
        {
            bool brandFilterIsNull = brandIdFilter == 0;
            bool typeFilterIsNull = typeIdFilter == 0;
            return _catalogItems.Where(x =>
                (brandFilterIsNull || x.CatalogBrandId == brandIdFilter) &&
                (typeFilterIsNull || x.CatalogTypeId == typeIdFilter)).ToList();
        }

        public IEnumerable<CatalogType> GetCatalogTypes()
        {
            return PreconfiguredData.GetPreconfiguredCatalogTypes();
        }

        public IEnumerable<CatalogBrand> GetCatalogBrands()
        {
            return PreconfiguredData.GetPreconfiguredCatalogBrands();
        }

        public void CreateCatalogItem(CatalogItem catalogItem)
        {
            var maxId = _catalogItems.Max(i => i.Id);
            catalogItem.Id = ++maxId;
            _catalogItems.Add(catalogItem);
        }

        public void UpdateCatalogItem(CatalogItem modifiedItem)
        {
            var originalItem = FindCatalogItem(modifiedItem.Id);
            if (originalItem != null)
            {
                _catalogItems[_catalogItems.IndexOf(originalItem)] = modifiedItem;
            }
        }

        public void RemoveCatalogItem(CatalogItem catalogItem)
        {
            _catalogItems.Remove(catalogItem);
        }

        public void Dispose()
        {
        }

        List<CatalogBrand> ICatalogService.GetCatalogBrands()
        {
            return _catalogBrands;
        }

        List<CatalogType> ICatalogService.GetCatalogTypes()
        {
            return _catalogTypes;
        }

        public int GetAvailableStock(DateTime date, int catalogItemId)
        {
            var s = _catalogItemsStock.FirstOrDefault(x => x.CatalogItemId == catalogItemId && x.Date.Date == date.Date);
            return s?.AvailableStock ?? 0;
        }

        public void CreateAvailableStock(CatalogItemsStock cat)
        {
            CatalogItemsStock? s = _catalogItemsStock
                .Where(x => x.CatalogItemId == cat.CatalogItemId)
                .FirstOrDefault(y => y.Date.Date == cat.Date.Date);

            if (s != null)
            {
                s.AvailableStock = cat.AvailableStock;
            }
            else
            {
                var maxId = _catalogItemsStock.Any() ? _catalogItemsStock.Max(i => i.StockId) : 0;
                cat.StockId = ++maxId;
                _catalogItemsStock.Add(cat);
            }
        }

        public DiscountItem GetDiscount(DateTime day)
        {
            throw new NotImplementedException();
        }
    }
}
