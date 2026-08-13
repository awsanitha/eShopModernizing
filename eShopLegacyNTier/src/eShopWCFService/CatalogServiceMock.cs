using eShopWCFService.Models;
using eShopWCFService.Models.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eShopWCFService
{
    public class CatalogServiceMock : ICatalogService
    {
        private List<CatalogItem> catalogItems;
        private List<CatalogBrand> catalogBrands;
        private List<CatalogType> catalogTypes;
        private List<CatalogItemsStock> catalogItemsStock;

        public CatalogServiceMock()
        {
            catalogItems = new List<CatalogItem>(PreconfiguredData.GetPreconfiguredCatalogItems());
            catalogBrands = new List<CatalogBrand>(PreconfiguredData.GetPreconfiguredCatalogBrands());
            catalogTypes = new List<CatalogType>(PreconfiguredData.GetPreconfiguredCatalogTypes());
            catalogItemsStock = new List<CatalogItemsStock>(PreconfiguredData.GetPreconfiguredCatalogItemsStock());
        }

        public CatalogItem FindCatalogItem(int id)
        {
            return catalogItems.FirstOrDefault(x => x.Id == id)!;
        }

        public List<CatalogItem> GetCatalogItems(int brandIdFilter, int typeIdFilter)
        {
            bool brandFilterIsNull = brandIdFilter == 0;
            bool typeFilterIsNull = typeIdFilter == 0;
            return catalogItems.Where(x =>
                (brandFilterIsNull || x.CatalogBrandId == brandIdFilter) &&
                (typeFilterIsNull || x.CatalogTypeId == typeIdFilter)).ToList();
        }

        public List<CatalogType> GetCatalogTypes()
        {
            return catalogTypes;
        }

        public List<CatalogBrand> GetCatalogBrands()
        {
            return catalogBrands;
        }

        public void CreateCatalogItem(CatalogItem catalogItem)
        {
            var maxId = catalogItems.Max(i => i.Id);
            catalogItem.Id = ++maxId;
            catalogItems.Add(catalogItem);
        }

        public void UpdateCatalogItem(CatalogItem modifiedItem)
        {
            var originalItem = FindCatalogItem(modifiedItem.Id);
            if (originalItem != null)
                catalogItems[catalogItems.IndexOf(originalItem)] = modifiedItem;
        }

        public void RemoveCatalogItem(CatalogItem catalogItem)
        {
            catalogItems.Remove(catalogItem);
        }

        public void Dispose()
        {
        }

        public int GetAvailableStock(DateTime date, int catalogItemId)
        {
            var s = catalogItemsStock.FirstOrDefault(
                x => x.CatalogItemId == catalogItemId && x.Date.Date == date.Date);
            return s?.AvailableStock ?? 0;
        }

        public void CreateAvailableStock(CatalogItemsStock cat)
        {
            CatalogItemsStock? s = catalogItemsStock
                .Where(x => x.CatalogItemId == cat.CatalogItemId)
                .FirstOrDefault(y => y.Date.Date == cat.Date.Date);

            if (s != null)
                s.AvailableStock = cat.AvailableStock;
            else
            {
                var maxId = catalogItemsStock.Max(i => i.StockId);
                cat.StockId = ++maxId;
                catalogItemsStock.Add(cat);
            }
        }

        public DiscountItem GetDiscount(DateTime day)
        {
            throw new NotImplementedException();
        }
    }
}
