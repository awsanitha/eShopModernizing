using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using eShopLegacyWebForms.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace eShopLegacyWebForms.Controllers
{
    public class CatalogController : Controller
    {
        private const int DefaultPageIndex = 0;
        private const int DefaultPageSize = 10;

        private readonly ICatalogService _catalogService;
        private readonly ILogger<CatalogController> _logger;

        public CatalogController(ICatalogService catalogService, ILogger<CatalogController> logger)
        {
            _catalogService = catalogService;
            _logger = logger;
        }

        // GET: /Catalog or /Default
        [HttpGet]
        [Route("")]
        [Route("Default")]
        [Route("Default/index/{index:int}/size/{size:int}", Name = "ProductsByPageRoute")]
        public IActionResult Index(int index = DefaultPageIndex, int size = DefaultPageSize)
        {
            _logger.LogInformation("Now loading... /Default?size={Size}&index={Index}", size, index);
            var model = _catalogService.GetCatalogItemsPaginated(size, index);
            return View(model);
        }

        // GET: /Catalog/Details/5
        [HttpGet]
        [Route("Catalog/Details/{id:int}", Name = "ProductDetailsRoute")]
        public IActionResult Details(int id)
        {
            _logger.LogInformation("Now loading... /Catalog/Details/{Id}", id);
            var product = _catalogService.FindCatalogItem(id);
            if (product == null)
                return NotFound();
            return View(product);
        }

        // GET: /Catalog/Create
        [HttpGet]
        [Route("Catalog/Create", Name = "CreateProductRoute")]
        public IActionResult Create()
        {
            _logger.LogInformation("Now loading... /Catalog/Create");
            ViewBag.Brands = _catalogService.GetCatalogBrands();
            ViewBag.Types = _catalogService.GetCatalogTypes();
            return View();
        }

        // POST: /Catalog/Create
        [HttpPost]
        [Route("Catalog/Create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(
            string name,
            string description,
            int brandId,
            int typeId,
            decimal price,
            int stock,
            int restock,
            int maxstock)
        {
            if (ModelState.IsValid)
            {
                var catalogItem = new CatalogItem
                {
                    Name = name,
                    Description = description,
                    CatalogBrandId = brandId,
                    CatalogTypeId = typeId,
                    Price = price,
                    AvailableStock = stock,
                    RestockThreshold = restock,
                    MaxStockThreshold = maxstock
                };

                _catalogService.CreateCatalogItem(catalogItem);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Brands = _catalogService.GetCatalogBrands();
            ViewBag.Types = _catalogService.GetCatalogTypes();
            return View();
        }

        // GET: /Catalog/Edit/5
        [HttpGet]
        [Route("Catalog/Edit/{id:int}", Name = "EditProductRoute")]
        public IActionResult Edit(int id)
        {
            _logger.LogInformation("Now loading... /Catalog/Edit/{Id}", id);
            var product = _catalogService.FindCatalogItem(id);
            if (product == null)
                return NotFound();

            ViewBag.Brands = _catalogService.GetCatalogBrands();
            ViewBag.Types = _catalogService.GetCatalogTypes();
            return View(product);
        }

        // POST: /Catalog/Edit/5
        [HttpPost]
        [Route("Catalog/Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(
            int id,
            string name,
            string description,
            int brandId,
            int typeId,
            decimal price,
            string pictureFileName,
            int stock,
            int restock,
            int maxstock)
        {
            if (ModelState.IsValid)
            {
                var catalogItem = new CatalogItem
                {
                    Id = id,
                    Name = name,
                    Description = description,
                    CatalogBrandId = brandId,
                    CatalogTypeId = typeId,
                    Price = price,
                    PictureFileName = pictureFileName,
                    AvailableStock = stock,
                    RestockThreshold = restock,
                    MaxStockThreshold = maxstock
                };

                _catalogService.UpdateCatalogItem(catalogItem);
                return RedirectToAction(nameof(Index));
            }

            var product = _catalogService.FindCatalogItem(id);
            ViewBag.Brands = _catalogService.GetCatalogBrands();
            ViewBag.Types = _catalogService.GetCatalogTypes();
            return View(product);
        }

        // GET: /Catalog/Delete/5
        [HttpGet]
        [Route("Catalog/Delete/{id:int}", Name = "DeleteProductRoute")]
        public IActionResult Delete(int id)
        {
            _logger.LogInformation("Now loading... /Catalog/Delete/{Id}", id);
            var product = _catalogService.FindCatalogItem(id);
            if (product == null)
                return NotFound();
            return View(product);
        }

        // POST: /Catalog/Delete/5
        [HttpPost]
        [Route("Catalog/Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var product = _catalogService.FindCatalogItem(id);
            if (product != null)
            {
                _catalogService.RemoveCatalogItem(product);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
