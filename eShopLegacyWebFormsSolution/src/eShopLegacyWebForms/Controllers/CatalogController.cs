using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using eShopLegacyWebForms.ViewModel;
using log4net;

namespace eShopLegacyWebForms.Controllers
{
    public class CatalogController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly ICatalogService _service;

        public CatalogController(ICatalogService service)
        {
            _service = service;
        }

        public IActionResult Index(int pageSize = 10, int pageIndex = 0)
        {
            _log.Info($"Loading catalog index pageSize={pageSize}&pageIndex={pageIndex}");
            var paginatedItems = _service.GetCatalogItemsPaginated(pageSize, pageIndex);
            return View(paginatedItems);
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return BadRequest();
            var item = _service.FindCatalogItem(id.Value);
            if (item == null) return NotFound();
            return View(item);
        }

        public IActionResult Create()
        {
            ViewBag.CatalogBrandId = new SelectList(_service.GetCatalogBrands(), "Id", "Brand");
            ViewBag.CatalogTypeId = new SelectList(_service.GetCatalogTypes(), "Id", "Type");
            return View(new CatalogItem());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,Name,Description,Price,PictureFileName,CatalogTypeId,CatalogBrandId,AvailableStock,RestockThreshold,MaxStockThreshold,OnReorder")] CatalogItem catalogItem)
        {
            if (ModelState.IsValid)
            {
                _service.CreateCatalogItem(catalogItem);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CatalogBrandId = new SelectList(_service.GetCatalogBrands(), "Id", "Brand", catalogItem.CatalogBrandId);
            ViewBag.CatalogTypeId = new SelectList(_service.GetCatalogTypes(), "Id", "Type", catalogItem.CatalogTypeId);
            return View(catalogItem);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null) return BadRequest();
            var item = _service.FindCatalogItem(id.Value);
            if (item == null) return NotFound();
            ViewBag.CatalogBrandId = new SelectList(_service.GetCatalogBrands(), "Id", "Brand", item.CatalogBrandId);
            ViewBag.CatalogTypeId = new SelectList(_service.GetCatalogTypes(), "Id", "Type", item.CatalogTypeId);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit([Bind("Id,Name,Description,Price,PictureFileName,CatalogTypeId,CatalogBrandId,AvailableStock,RestockThreshold,MaxStockThreshold,OnReorder")] CatalogItem catalogItem)
        {
            if (ModelState.IsValid)
            {
                _service.UpdateCatalogItem(catalogItem);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CatalogBrandId = new SelectList(_service.GetCatalogBrands(), "Id", "Brand", catalogItem.CatalogBrandId);
            ViewBag.CatalogTypeId = new SelectList(_service.GetCatalogTypes(), "Id", "Type", catalogItem.CatalogTypeId);
            return View(catalogItem);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null) return BadRequest();
            var item = _service.FindCatalogItem(id.Value);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _service.FindCatalogItem(id);
            if (item != null) _service.RemoveCatalogItem(item);
            return RedirectToAction(nameof(Index));
        }
    }
}
