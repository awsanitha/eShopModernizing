using eShopWCFService.Models;
using Microsoft.EntityFrameworkCore;

namespace eShopWCFService
{
    public class CatalogService : ICatalogService
    {
        private readonly EntityModel _ents;

        public CatalogService(EntityModel ents)
        {
            _ents = ents;
        }

        public DiscountItem GetDiscount(DateTime _day)
        {
            return _ents.DiscountItems.ToList().Where(y => y.Start.Date <= _day.Date && y.End.Date >= _day.Date).FirstOrDefault()!;
        }

        public CatalogItem FindCatalogItem(int id)
        {
            CatalogItem? item = _ents.CatalogItems
                .Include(x => x.CatalogBrand)
                .Include(x => x.CatalogType)
                .FirstOrDefault(x => x.Id == id);
            return item!;
        }

        public List<CatalogType> GetCatalogTypes()
        {
            return _ents.CatalogTypes.ToList();
        }

        public List<CatalogBrand> GetCatalogBrands()
        {
            return _ents.CatalogBrands.ToList();
        }

        public List<CatalogItem> GetCatalogItems(int brandIdFilter, int typeIdFilter)
        {
            bool brandFilterIsNull = brandIdFilter == 0;
            bool typeFilterIsNull = typeIdFilter == 0;
            return _ents.CatalogItems
                .Include(x => x.CatalogBrand)
                .Include(x => x.CatalogType)
                .ToList()
                .Where(x =>
                    (brandFilterIsNull || x.CatalogBrandId == brandIdFilter) &&
                    (typeFilterIsNull || x.CatalogTypeId == typeIdFilter))
                .ToList();
        }

        public void CreateCatalogItem(CatalogItem catalogItem)
        {
            var maxId = _ents.CatalogItems.Any() ? _ents.CatalogItems.Max(i => i.Id) : 0;
            catalogItem.Id = ++maxId;
            _ents.CatalogItems.Add(catalogItem);
            _ents.SaveChanges();
        }

        public void UpdateCatalogItem(CatalogItem catalogItem)
        {
            _ents.Entry(catalogItem).State = EntityState.Modified;
            _ents.SaveChanges();
        }

        public void RemoveCatalogItem(CatalogItem catalogItem)
        {
            _ents.CatalogItems.Remove(catalogItem);
            _ents.SaveChanges();
        }

        public void Dispose()
        {
            _ents.Dispose();
        }

        public int GetAvailableStock(DateTime date, int catalogItemId)
        {
            CatalogItemsStock? s = _ents.CatalogItemsStocks
                .Where(x => x.CatalogItemId == catalogItemId)
                .ToList()
                .Where(y => y.Date.Date == date.Date)
                .FirstOrDefault();
            return s?.AvailableStock ?? 0;
        }

        public void CreateAvailableStock(CatalogItemsStock catalogItemsStock)
        {
            CatalogItemsStock? s = _ents.CatalogItemsStocks
                .Where(x => x.CatalogItemId == catalogItemsStock.CatalogItemId)
                .ToList()
                .Where(y => y.Date.Date == catalogItemsStock.Date.Date)
                .FirstOrDefault();

            if (s != null)
            {
                s.AvailableStock = catalogItemsStock.AvailableStock;
                _ents.Entry(s).State = EntityState.Modified;
                _ents.SaveChanges();
            }
            else
            {
                var maxId = _ents.CatalogItemsStocks.Any() ? _ents.CatalogItemsStocks.Max(i => i.StockId) : 0;
                catalogItemsStock.StockId = ++maxId;
                _ents.CatalogItemsStocks.Add(catalogItemsStock);
                _ents.SaveChanges();
            }
        }
    }
}
