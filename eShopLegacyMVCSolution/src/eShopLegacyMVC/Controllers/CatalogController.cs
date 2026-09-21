using eShopLegacyMVC.Models;
using eShopLegacyMVC.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection;

namespace eShopLegacyMVC.Controllers
{
    public class CatalogController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly ICatalogService _service;

        public CatalogController(ICatalogService service)
        {
            _service = service;
        }

        public IActionResult Index(int pageSize = 10, int pageIndex = 0)
        {
            var catalogModel = _service.GetCatalogItemsPaginated(pageSize, pageIndex);
            AddUriPlaceHolder(catalogModel.Data);
            return View(catalogModel);
        }

        public IActionResult Details(int id)
        {
            var item = _service.FindCatalogItem(id);
            if (item == null) return NotFound();
            AddUriPlaceHolder(new[] { item });
            return View(item);
        }

        public IActionResult Create()
        {
            ViewBag.CatalogBrandId = new SelectList(_service.GetCatalogBrands(), "Id", "Brand");
            ViewBag.CatalogTypeId = new SelectList(_service.GetCatalogTypes(), "Id", "Type");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CatalogItem catalogItem)
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

        public IActionResult Edit(int id)
        {
            var item = _service.FindCatalogItem(id);
            if (item == null) return NotFound();
            AddUriPlaceHolder(new[] { item });
            ViewBag.CatalogBrandId = new SelectList(_service.GetCatalogBrands(), "Id", "Brand", item.CatalogBrandId);
            ViewBag.CatalogTypeId = new SelectList(_service.GetCatalogTypes(), "Id", "Type", item.CatalogTypeId);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CatalogItem catalogItem)
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

        public IActionResult Delete(int id)
        {
            var item = _service.FindCatalogItem(id);
            if (item == null) return NotFound();
            AddUriPlaceHolder(new[] { item });
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _service.FindCatalogItem(id);
            if (item != null)
                _service.RemoveCatalogItem(item);
            return RedirectToAction(nameof(Index));
        }

        private void AddUriPlaceHolder(IEnumerable<CatalogItem> items)
        {
            foreach (var item in items)
            {
                item.PictureUri = Url.RouteUrl(PicController.GetPicRouteName, new { catalogItemId = item.Id });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _service.Dispose();
            base.Dispose(disposing);
        }
    }
}
