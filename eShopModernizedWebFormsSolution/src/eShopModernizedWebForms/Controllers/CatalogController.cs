using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using eShopModernizedWebForms.Models;
using eShopModernizedWebForms.Services;
using log4net;

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

        // GET /[?size=3&index=10]
        public ActionResult Index(int size = 10, int index = 0)
        {
            _log.Info($"Now loading... /Catalog/Index?size={size}&index={index}");
            var paginatedItems = _service.GetCatalogItemsPaginated(size, index);
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
            CatalogItem catalogItem = _service.FindCatalogItem(id.Value);
            if (catalogItem == null)
            {
                return NotFound();
            }
            AddUriPlaceHolder(catalogItem);

            return View(catalogItem);
        }

        // GET: Catalog/Create
        public ActionResult Create()
        {
            _log.Info($"Now loading... /Catalog/Create");
            ViewBag.CatalogBrandId = new SelectList(_service.GetCatalogBrands(), "Id", "Brand");
            ViewBag.CatalogTypeId = new SelectList(_service.GetCatalogTypes(), "Id", "Type");
            ViewBag.UseAzureStorage = CatalogConfiguration.UseAzureStorage;

            return View(new CatalogItem
            {
                PictureUri = _imageService.UrlDefaultImage()
            });
        }

        // POST: Catalog/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("Id,Name,Description,Price,PictureFileName,CatalogTypeId,CatalogBrandId,AvailableStock,RestockThreshold,MaxStockThreshold,OnReorder,TempImageName")] CatalogItem catalogItem)
        {
            _log.Info($"Now processing... /Catalog/Create?catalogItemName={catalogItem.Name}");
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(catalogItem.TempImageName))
                {
                    var fileName = Path.GetFileName(catalogItem.TempImageName);
                    catalogItem.PictureFileName = fileName;
                }

                _service.CreateCatalogItem(catalogItem);
                if (!string.IsNullOrEmpty(catalogItem.TempImageName))
                {
                    _imageService.UpdateImage(catalogItem);
                }
                return RedirectToAction("Index");
            }

            ViewBag.CatalogBrandId = new SelectList(_service.GetCatalogBrands(), "Id", "Brand", catalogItem.CatalogBrandId);
            ViewBag.CatalogTypeId = new SelectList(_service.GetCatalogTypes(), "Id", "Type", catalogItem.CatalogTypeId);
            ViewBag.UseAzureStorage = CatalogConfiguration.UseAzureStorage;
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
            CatalogItem catalogItem = _service.FindCatalogItem(id.Value);

            if (catalogItem == null)
            {
                return NotFound();
            }
            AddUriPlaceHolder(catalogItem);
            ViewBag.CatalogBrandId = new SelectList(_service.GetCatalogBrands(), "Id", "Brand", catalogItem.CatalogBrandId);
            ViewBag.CatalogTypeId = new SelectList(_service.GetCatalogTypes(), "Id", "Type", catalogItem.CatalogTypeId);
            ViewBag.UseAzureStorage = CatalogConfiguration.UseAzureStorage;
            return View(catalogItem);
        }

        // POST: Catalog/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind("Id,Name,Description,Price,PictureFileName,CatalogTypeId,CatalogBrandId,AvailableStock,RestockThreshold,MaxStockThreshold,OnReorder,TempImageName")] CatalogItem catalogItem)
        {
            _log.Info($"Now processing... /Catalog/Edit?id={catalogItem.Id}");
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(catalogItem.TempImageName))
                {
                    _imageService.UpdateImage(catalogItem);
                    var fileName = Path.GetFileName(catalogItem.TempImageName);
                    catalogItem.PictureFileName = fileName;
                }

                _service.UpdateCatalogItem(catalogItem);
                return RedirectToAction("Index");
            }
            ViewBag.CatalogBrandId = new SelectList(_service.GetCatalogBrands(), "Id", "Brand", catalogItem.CatalogBrandId);
            ViewBag.CatalogTypeId = new SelectList(_service.GetCatalogTypes(), "Id", "Type", catalogItem.CatalogTypeId);
            ViewBag.UseAzureStorage = CatalogConfiguration.UseAzureStorage;
            return View(catalogItem);
        }

        // GET: Catalog/Delete/5
        public ActionResult Delete(int? id)
        {
            _log.Info($"Now loading... /Catalog/Delete?id={id}");
            if (id == null)
            {
                return BadRequest();
            }
            CatalogItem catalogItem = _service.FindCatalogItem(id.Value);
            if (catalogItem == null)
            {
                return NotFound();
            }
            AddUriPlaceHolder(catalogItem);

            return View(catalogItem);
        }

        // POST: Catalog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            _log.Info($"Now processing... /Catalog/DeleteConfirmed?id={id}");
            CatalogItem catalogItem = _service.FindCatalogItem(id);
            _service.RemoveCatalogItem(catalogItem);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _service.Dispose();
            }
            base.Dispose(disposing);
        }

        private void ChangeUriPlaceholder(IEnumerable<CatalogItem> items)
        {
            foreach (var catalogItem in items)
            {
                AddUriPlaceHolder(catalogItem);
            }
        }

        private void AddUriPlaceHolder(CatalogItem item)
        {
            item.PictureUri = _imageService.BuildUrlImage(item);
        }
    }
}
