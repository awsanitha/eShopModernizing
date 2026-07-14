using System;
using System.Collections.Generic;
using eShopWCFService.Models;

namespace eShopWCFService
{
    /// <summary>
    /// Client proxy stub for the catalog service (not used server-side; kept for API compatibility).
    /// </summary>
    public class CatalogServiceClient : ICatalogService
    {
        public void CreateAvailableStock(CatalogItemsStock catalogItemsStock)
        {
            throw new NotImplementedException();
        }

        public void CreateCatalogItem(CatalogItem catalogItem)
        {
            throw new NotImplementedException();
        }

        public CatalogItem FindCatalogItem(int id)
        {
            throw new NotImplementedException();
        }

        public int GetAvailableStock(DateTime date, int catalogItemId)
        {
            throw new NotImplementedException();
        }

        public List<CatalogBrand> GetCatalogBrands()
        {
            throw new NotImplementedException();
        }

        public List<CatalogItem> GetCatalogItems(int brandIdFilter, int typeIdFilter)
        {
            throw new NotImplementedException();
        }

        public List<CatalogType> GetCatalogTypes()
        {
            throw new NotImplementedException();
        }

        public void RemoveCatalogItem(CatalogItem catalogItem)
        {
            throw new NotImplementedException();
        }

        public void UpdateCatalogItem(CatalogItem catalogItem)
        {
            throw new NotImplementedException();
        }

        public DiscountItem GetDiscount(DateTime day)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
