using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using log4net;

namespace eShopLegacyWebForms.Controllers
{
    public class CatalogController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ICatalogService service;

        public CatalogController(ICatalogService service)
        {
            this.service = service;
        }

        // GET /[?pageSize=3&pageIndex=10]
        public ActionResult Index(int size = 10, int index = 0)
        {
            _log.Info($"Now loading... /Catalog/Index?size={size}&index={index}");
            var paginatedItems = service.GetCatalogItemsPaginated(size, index);
            ChangeUriPlaceholder(paginatedItems.Data);
            return View(paginatedItems);
        }

        // GET: Catalog/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            _log.Info($"Now loading... /Catalog/Details?id={id}");
            CatalogItem catalogItem = service.FindCatalogItem(id.Value);
            if (catalogItem == null)
            {
                return NotFound();
            }

            return View(catalogItem);
        }

        // GET: Catalog/Create
        public ActionResult Create()
        {
            _log.Info($"Now loading... /Catalog/Create");
            ViewBag.CatalogBrandId = new SelectList(service.GetCatalogBrands(), "Id", "Brand");
            ViewBag.CatalogTypeId = new SelectList(service.GetCatalogTypes(), "Id", "Type");
            return View(new CatalogItem());
        }

        // POST: Catalog/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("Id,Name,Description,Price,PictureFileName,CatalogTypeId,CatalogBrandId,AvailableStock,RestockThreshold,MaxStockThreshold,OnReorder")] CatalogItem catalogItem)
        {
            _log.Info($"Now processing... /Catalog/Create?catalogItemName={catalogItem.Name}");
            if (ModelState.IsValid)
            {
                service.CreateCatalogItem(catalogItem);
                return RedirectToAction("Index");
            }

            ViewBag.CatalogBrandId = new SelectList(service.GetCatalogBrands(), "Id", "Brand", catalogItem.CatalogBrandId);
            ViewBag.CatalogTypeId = new SelectList(service.GetCatalogTypes(), "Id", "Type", catalogItem.CatalogTypeId);
            return View(catalogItem);
        }

        // GET: Catalog/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            _log.Info($"Now loading... /Catalog/Edit?id={id}");
            CatalogItem catalogItem = service.FindCatalogItem(id.Value);
            if (catalogItem == null)
            {
                return NotFound();
            }
            ViewBag.CatalogBrandId = new SelectList(service.GetCatalogBrands(), "Id", "Brand", catalogItem.CatalogBrandId);
            ViewBag.CatalogTypeId = new SelectList(service.GetCatalogTypes(), "Id", "Type", catalogItem.CatalogTypeId);
            return View(catalogItem);
        }

        // POST: Catalog/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind("Id,Name,Description,Price,PictureFileName,CatalogTypeId,CatalogBrandId,AvailableStock,RestockThreshold,MaxStockThreshold,OnReorder")] CatalogItem catalogItem)
        {
            _log.Info($"Now processing... /Catalog/Edit?id={catalogItem.Id}");
            if (ModelState.IsValid)
            {
                service.UpdateCatalogItem(catalogItem);
                return RedirectToAction("Index");
            }
            ViewBag.CatalogBrandId = new SelectList(service.GetCatalogBrands(), "Id", "Brand", catalogItem.CatalogBrandId);
            ViewBag.CatalogTypeId = new SelectList(service.GetCatalogTypes(), "Id", "Type", catalogItem.CatalogTypeId);
            return View(catalogItem);
        }

        // GET: Catalog/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            _log.Info($"Now loading... /Catalog/Delete?id={id}");
            CatalogItem catalogItem = service.FindCatalogItem(id.Value);
            if (catalogItem == null)
            {
                return NotFound();
            }

            return View(catalogItem);
        }

        // POST: Catalog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _log.Info($"Now processing... /Catalog/DeleteConfirmed?id={id}");
            CatalogItem catalogItem = service.FindCatalogItem(id);
            service.RemoveCatalogItem(catalogItem);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                service.Dispose();
            }
            base.Dispose(disposing);
        }

        private void ChangeUriPlaceholder(IEnumerable<CatalogItem> items)
        {
            foreach (var catalogItem in items)
            {
                catalogItem.PictureUri = $"/Pics/{catalogItem.PictureFileName}";
            }
        }
    }
}
