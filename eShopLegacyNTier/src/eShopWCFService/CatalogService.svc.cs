using CoreWCF;
using eShopWCFService.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eShopWCFService
{
    public class CatalogService : ICatalogService
    {
        private readonly EntityModel ents;

        public CatalogService(EntityModel ents)
        {
            this.ents = ents;
        }

        public DiscountItem? GetDiscount(DateTime _day)
        {
            return ents.DiscountItems.ToList()
                .Where(y => y.Start.Date <= _day.Date && y.End.Date >= _day.Date)
                .FirstOrDefault();
        }

        public CatalogItem? FindCatalogItem(int id)
        {
            CatalogItem? item = ents.CatalogItems.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                item.CatalogBrand = ents.CatalogBrands.FirstOrDefault(x => x.Id == item.CatalogBrandId);
                item.CatalogType = ents.CatalogTypes.FirstOrDefault(x => x.Id == item.CatalogTypeId);
            }
            return item;
        }

        public List<CatalogType> GetCatalogTypes()
        {
            return ents.CatalogTypes.ToList();
        }

        public List<CatalogBrand> GetCatalogBrands()
        {
            return ents.CatalogBrands.ToList();
        }

        public List<CatalogItem> GetCatalogItems(int brandIdFilter, int typeIdFilter)
        {
            bool brandFilterIsNull = brandIdFilter == 0;
            bool typeFilterIsNull = typeIdFilter == 0;
            return ents.CatalogItems.ToList().Where(x =>
                (brandFilterIsNull || x.CatalogBrandId == brandIdFilter) &&
                (typeFilterIsNull || x.CatalogTypeId == typeIdFilter)).ToList();
        }

        public void CreateCatalogItem(CatalogItem catalogItem)
        {
            var maxId = ents.CatalogItems.Any() ? ents.CatalogItems.Max(i => i.Id) : 0;
            catalogItem.Id = ++maxId;
            ents.CatalogItems.Add(catalogItem);
            ents.SaveChanges();
        }

        public void UpdateCatalogItem(CatalogItem catalogItem)
        {
            ents.Entry(catalogItem).State = EntityState.Modified;
            ents.SaveChanges();
        }

        public void RemoveCatalogItem(CatalogItem catalogItem)
        {
            ents.CatalogItems.Remove(catalogItem);
            ents.SaveChanges();
        }

        public void Dispose()
        {
            ents.Dispose();
        }

        public int GetAvailableStock(DateTime date, int catalogItemId)
        {
            CatalogItemsStock? s = ents.CatalogItemsStocks
                .Where(x => x.CatalogItemId == catalogItemId)
                .ToList()
                .Where(y => y.Date.Date == date.Date)
                .FirstOrDefault();
            return s?.AvailableStock ?? 0;
        }

        public void CreateAvailableStock(CatalogItemsStock catalogItemsStock)
        {
            CatalogItemsStock? s = ents.CatalogItemsStocks
                .Where(x => x.CatalogItemId == catalogItemsStock.CatalogItemId)
                .ToList()
                .Where(y => y.Date.Date == catalogItemsStock.Date.Date)
                .FirstOrDefault();

            if (s != null)
            {
                s.AvailableStock = catalogItemsStock.AvailableStock;
                ents.Entry(s).State = EntityState.Modified;
                ents.SaveChanges();
            }
            else
            {
                var maxId = ents.CatalogItemsStocks.Any() ? ents.CatalogItemsStocks.Max(i => i.StockId) : 0;
                catalogItemsStock.StockId = ++maxId;
                ents.CatalogItemsStocks.Add(catalogItemsStock);
                ents.SaveChanges();
            }
        }
    }
}
