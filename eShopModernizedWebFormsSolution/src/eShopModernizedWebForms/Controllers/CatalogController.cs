using eShopModernizedWebForms.Models;
using eShopModernizedWebForms.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.IO;

namespace eShopModernizedWebForms.Controllers
{
    public class CatalogController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ICatalogService _service;
        private readonly IImageService _imageService;

        public CatalogController(ICatalogService service, IImageService imageService)
        {
            _service = service;
            _imageService = imageService;
        }

        public IActionResult Index(int pageSize = 10, int pageIndex = 0)
        {
            _log.Info($"Now loading... /Catalog/Index?pageSize={pageSize}&pageIndex={pageIndex}");
            var model = _service.GetCatalogItemsPaginated(pageSize, pageIndex);
            ChangeUriPlaceholder(model.Data);
            return View(model);
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return BadRequest();
            var item = _service.FindCatalogItem(id.Value);
            if (item == null) return NotFound();
            AddUriPlaceHolder(item);
            return View(item);
        }

        public IActionResult Create()
        {
            ViewBag.CatalogBrandId = new SelectList(_service.GetCatalogBrands(), "Id", "Brand");
            ViewBag.CatalogTypeId = new SelectList(_service.GetCatalogTypes(), "Id", "Type");
            ViewBag.UseAzureStorage = CatalogConfiguration.UseAzureStorage;
            return View(new CatalogItem { PictureUri = _imageService.UrlDefaultImage() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,Name,Description,Price,PictureFileName,CatalogTypeId,CatalogBrandId,AvailableStock,RestockThreshold,MaxStockThreshold,OnReorder,TempImageName")] CatalogItem catalogItem)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(catalogItem.TempImageName))
                    catalogItem.PictureFileName = Path.GetFileName(catalogItem.TempImageName);
                _service.CreateCatalogItem(catalogItem);
                if (!string.IsNullOrEmpty(catalogItem.TempImageName))
                    _imageService.UpdateImage(catalogItem);
                return RedirectToAction("Index");
            }
            ViewBag.CatalogBrandId = new SelectList(_service.GetCatalogBrands(), "Id", "Brand", catalogItem.CatalogBrandId);
            ViewBag.CatalogTypeId = new SelectList(_service.GetCatalogTypes(), "Id", "Type", catalogItem.CatalogTypeId);
            ViewBag.UseAzureStorage = CatalogConfiguration.UseAzureStorage;
            return View(catalogItem);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null) return BadRequest();
            var item = _service.FindCatalogItem(id.Value);
            if (item == null) return NotFound();
            AddUriPlaceHolder(item);
            ViewBag.CatalogBrandId = new SelectList(_service.GetCatalogBrands(), "Id", "Brand", item.CatalogBrandId);
            ViewBag.CatalogTypeId = new SelectList(_service.GetCatalogTypes(), "Id", "Type", item.CatalogTypeId);
            ViewBag.UseAzureStorage = CatalogConfiguration.UseAzureStorage;
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit([Bind("Id,Name,Description,Price,PictureFileName,CatalogTypeId,CatalogBrandId,AvailableStock,RestockThreshold,MaxStockThreshold,OnReorder,TempImageName")] CatalogItem catalogItem)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(catalogItem.TempImageName))
                {
                    _imageService.UpdateImage(catalogItem);
                    catalogItem.PictureFileName = Path.GetFileName(catalogItem.TempImageName);
                }
                _service.UpdateCatalogItem(catalogItem);
                return RedirectToAction("Index");
            }
            ViewBag.CatalogBrandId = new SelectList(_service.GetCatalogBrands(), "Id", "Brand", catalogItem.CatalogBrandId);
            ViewBag.CatalogTypeId = new SelectList(_service.GetCatalogTypes(), "Id", "Type", catalogItem.CatalogTypeId);
            ViewBag.UseAzureStorage = CatalogConfiguration.UseAzureStorage;
            return View(catalogItem);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null) return BadRequest();
            var item = _service.FindCatalogItem(id.Value);
            if (item == null) return NotFound();
            AddUriPlaceHolder(item);
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _service.FindCatalogItem(id);
            _service.RemoveCatalogItem(item);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _service.Dispose();
            base.Dispose(disposing);
        }

        private void ChangeUriPlaceholder(IEnumerable<CatalogItem> items)
        {
            foreach (var item in items) AddUriPlaceHolder(item);
        }

        private void AddUriPlaceHolder(CatalogItem item)
        {
            item.PictureUri = _imageService.BuildUrlImage(item);
        }
    }
}
