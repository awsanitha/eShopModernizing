using eShopWCFService.Models;
using System.Collections.Generic;
using System.Runtime.Serialization;
using CoreWCF;

namespace eShopWCFService
{
    // NOTE: Service contract for the eShop catalog WCF service (migrated from System.ServiceModel to CoreWCF)
    [ServiceContract]
    public interface ICatalogService
    {
        [OperationContract]
        CatalogItem FindCatalogItem(int id);
        [OperationContract]
        List<CatalogBrand> GetCatalogBrands();
        [OperationContract]
        List<CatalogItem> GetCatalogItems(int brandIdFilter, int typeIdFilter);
        [OperationContract]
        List<CatalogType> GetCatalogTypes();
        [OperationContract]
        int GetAvailableStock(System.DateTime date, int catalogItemId);
        [OperationContract]
        void CreateAvailableStock(CatalogItemsStock catalogItemsStock);
        [OperationContract]
        void CreateCatalogItem(CatalogItem catalogItem);
        [OperationContract]
        void UpdateCatalogItem(CatalogItem catalogItem);
        [OperationContract]
        void RemoveCatalogItem(CatalogItem catalogItem);
        [OperationContract]
        DiscountItem GetDiscount(DateTime day);
    }
}
