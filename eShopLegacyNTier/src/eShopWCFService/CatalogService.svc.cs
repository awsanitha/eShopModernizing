using eShopWCFService.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

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
            return _ents.DiscountItems.ToList()
                .Where(y => y.Start.Date.Date <= _day.Date.Date && y.End.Date.Date >= _day.Date.Date)
                .FirstOrDefault()!;
        }

        public CatalogItem FindCatalogItem(int id)
        {
            CatalogItem? item = _ents.CatalogItems.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                item.CatalogBrand = _ents.CatalogBrands.FirstOrDefault(x => x.Id == item.CatalogBrandId)!;
                item.CatalogType = _ents.CatalogTypes.FirstOrDefault(x => x.Id == item.CatalogTypeId)!;
                return item;
            }
            else
            {
                return null!;
            }
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
            return _ents.CatalogItems.ToList().Where(x =>
                (brandFilterIsNull ? true : x.CatalogBrandId == brandIdFilter) &&
                (typeFilterIsNull ? true : x.CatalogTypeId == typeIdFilter)).ToList();
        }

        public void CreateCatalogItem(CatalogItem catalogItem)
        {
            var maxId = _ents.CatalogItems.Max(i => i.Id);
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

            /* Overwrite the existing stock item for that date if we already have one for this item. Otherwise, make a new entry */
            if (s != null)
            {
                s.AvailableStock = catalogItemsStock.AvailableStock;
                _ents.Entry(s).State = EntityState.Modified;
                _ents.SaveChanges();
            }
            else
            {
                var maxId = _ents.CatalogItemsStocks.Max(i => i.StockId);
                catalogItemsStock.StockId = ++maxId;
                _ents.CatalogItemsStocks.Add(catalogItemsStock);
                _ents.SaveChanges();
            }
        }
    }
}
