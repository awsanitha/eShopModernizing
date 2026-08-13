using eShopWCFService.Models;

namespace eShopWCFService
{
    // This client is a server-side utility class; WinForms uses its own auto-generated proxy.
    public class CatalogServiceClient : System.ServiceModel.ClientBase<ICatalogService>, ICatalogService
    {
        public CatalogServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress address)
            : base(binding, address)
        {
        }

        public void CreateAvailableStock(CatalogItemsStock catalogItemsStock)
        {
            base.Channel.CreateAvailableStock(catalogItemsStock);
        }

        public void CreateCatalogItem(CatalogItem catalogItem)
        {
            base.Channel.CreateCatalogItem(catalogItem);
        }

        public CatalogItem FindCatalogItem(int id)
        {
            return base.Channel.FindCatalogItem(id);
        }

        public int GetAvailableStock(DateTime date, int catalogItemId)
        {
            return base.Channel.GetAvailableStock(date, catalogItemId);
        }

        public List<CatalogBrand> GetCatalogBrands()
        {
            return base.Channel.GetCatalogBrands();
        }

        public List<CatalogItem> GetCatalogItems(int brandIdFilter, int typeIdFilter)
        {
            return base.Channel.GetCatalogItems(brandIdFilter, typeIdFilter);
        }

        public List<CatalogType> GetCatalogTypes()
        {
            return base.Channel.GetCatalogTypes();
        }

        public void RemoveCatalogItem(CatalogItem catalogItem)
        {
            base.Channel.RemoveCatalogItem(catalogItem);
        }

        public void UpdateCatalogItem(CatalogItem catalogItem)
        {
            base.Channel.UpdateCatalogItem(catalogItem);
        }

        public DiscountItem GetDiscount(DateTime day)
        {
            return base.Channel.GetDiscount(day);
        }

        public void Dispose()
        {
            base.Channel.Dispose();
        }
    }
}
