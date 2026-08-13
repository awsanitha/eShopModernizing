using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using eShopModernizedWebForms.Models;
using eShopModernizedWebForms.Services;
using eShopModernizedWebForms.ViewModel;
using log4net;

namespace eShopModernizedWebForms.Controllers
{
    public class CatalogController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly ICatalogService _service;
        private readonly IImageService _imageService;

        public CatalogController(ICatalogService service, IImageService imageService)
        {
            _service = service;
            _imageService = imageService;
        }

        public IActionResult Index(int pageSize = 10, int pageIndex = 0)
        {
            _log.Info($"Loading catalog index pageSize={pageSize}&pageIndex={pageIndex}");
            var paginatedItems = _service.GetCatalogItemsPaginated(pageSize, pageIndex);
            ViewBag.BaseImageUrl = _imageService.BaseUrl();
            return View(paginatedItems);
        }

        public IActionResult Details(int? id)
        {
            if (id == null) return BadRequest();
            var item = _service.FindCatalogItem(id.Value);
            if (item == null) return NotFound();
            ViewBag.ImageUrl = _imageService.BuildUrlImage(item);
            return View(item);
        }

        public IActionResult Create()
        {
            ViewBag.CatalogBrandId = new SelectList(_service.GetCatalogBrands(), "Id", "Brand");
            ViewBag.CatalogTypeId = new SelectList(_service.GetCatalogTypes(), "Id", "Type");
            ViewBag.DefaultImageUrl = _imageService.UrlDefaultImage();
            return View(new CatalogItem());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,Name,Description,Price,PictureFileName,CatalogTypeId,CatalogBrandId,AvailableStock,RestockThreshold,MaxStockThreshold,OnReorder,TempImageName")] CatalogItem catalogItem)
        {
            if (ModelState.IsValid)
            {
                _imageService.UpdateImage(catalogItem);
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
            ViewBag.ImageUrl = _imageService.BuildUrlImage(item);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit([Bind("Id,Name,Description,Price,PictureFileName,CatalogTypeId,CatalogBrandId,AvailableStock,RestockThreshold,MaxStockThreshold,OnReorder,TempImageName")] CatalogItem catalogItem)
        {
            if (ModelState.IsValid)
            {
                _imageService.UpdateImage(catalogItem);
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

        [HttpPost]
        public IActionResult UploadImage(IFormFile file, int? itemId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var urlImageTemp = _imageService.UploadTempImage(file, itemId);
            return Json(new { name = urlImageTemp, url = urlImageTemp });
        }
    }
}
