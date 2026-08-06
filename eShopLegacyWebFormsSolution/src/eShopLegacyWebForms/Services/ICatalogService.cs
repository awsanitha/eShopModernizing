using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.ViewModel;
using System;
using System.Collections.Generic;

namespace eShopLegacyWebForms.Services
{
    public interface ICatalogService : IDisposable
    {
        CatalogItem? FindCatalogItem(int id);
        IEnumerable<CatalogBrand> GetCatalogBrands();
        PaginatedItemsViewModel<CatalogItem> GetCatalogItemsPaginated(int pageSize, int pageIndex);
        IEnumerable<CatalogType> GetCatalogTypes();
        void CreateCatalogItem(CatalogItem catalogItem);
        void UpdateCatalogItem(CatalogItem catalogItem);
        void RemoveCatalogItem(CatalogItem catalogItem);
    }
}
