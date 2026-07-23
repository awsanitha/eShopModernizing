using CoreWCF;
using eShopWCFService.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace eShopWCFService
{
    [ServiceContract]
    public interface ICatalogService : IDisposable
    {
        [OperationContract]
        CatalogItem? FindCatalogItem(int id);
        [OperationContract]
        List<CatalogBrand> GetCatalogBrands();
        [OperationContract]
        List<CatalogItem> GetCatalogItems(int brandIdFilter, int typeIdFilter);
        [OperationContract]
        List<CatalogType> GetCatalogTypes();
        [OperationContract]
        int GetAvailableStock(DateTime date, int catalogItemId);
        [OperationContract]
        void CreateAvailableStock(CatalogItemsStock catalogItemsStock);
        [OperationContract]
        void CreateCatalogItem(CatalogItem catalogItem);
        [OperationContract]
        void UpdateCatalogItem(CatalogItem catalogItem);
        [OperationContract]
        void RemoveCatalogItem(CatalogItem catalogItem);
        [OperationContract]
        DiscountItem? GetDiscount(DateTime day);
    }
}
