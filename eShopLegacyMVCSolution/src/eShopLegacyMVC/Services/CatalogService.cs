using eShopLegacyMVC.Models;
using eShopLegacyMVC.ViewModel;
using log4net;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace eShopLegacyMVC.Services
{
    public class CatalogService : ICatalogService
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly CatalogDBContext _db;
        private readonly CatalogItemHiLoGenerator _hiLoGenerator;

        public CatalogService(CatalogDBContext db, CatalogItemHiLoGenerator hiLoGenerator)
        {
            _db = db;
            _hiLoGenerator = hiLoGenerator;
        }

        public PaginatedItemsViewModel<CatalogItem> GetCatalogItemsPaginated(int pageSize, int pageIndex)
        {
            _log.Debug($"GetCatalogItemsPaginated called pageSize={pageSize} pageIndex={pageIndex}");

            var totalItems = _db.CatalogItems.LongCount();
            var itemsOnPage = _db.CatalogItems
                .Include(c => c.CatalogBrand)
                .Include(c => c.CatalogType)
                .OrderBy(c => c.Id)
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToList();

            return new PaginatedItemsViewModel<CatalogItem>(pageIndex, pageSize, totalItems, itemsOnPage);
        }

        public CatalogItem? FindCatalogItem(int id)
        {
            return _db.CatalogItems
                .Include(c => c.CatalogBrand)
                .Include(c => c.CatalogType)
                .FirstOrDefault(c => c.Id == id);
        }

        public IEnumerable<CatalogBrand> GetCatalogBrands()
        {
            return _db.CatalogBrands.ToList();
        }

        public IEnumerable<CatalogType> GetCatalogTypes()
        {
            return _db.CatalogTypes.ToList();
        }

        public void CreateCatalogItem(CatalogItem catalogItem)
        {
            catalogItem.Id = _hiLoGenerator.GetNextSequenceValue(_db);
            _db.CatalogItems.Add(catalogItem);
            _db.SaveChanges();
        }

        public void UpdateCatalogItem(CatalogItem catalogItem)
        {
            _db.Entry(catalogItem).State = EntityState.Modified;
            _db.SaveChanges();
        }

        public void RemoveCatalogItem(CatalogItem catalogItem)
        {
            _db.CatalogItems.Remove(catalogItem);
            _db.SaveChanges();
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}
