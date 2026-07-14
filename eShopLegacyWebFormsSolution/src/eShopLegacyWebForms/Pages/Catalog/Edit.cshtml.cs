using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace eShopLegacyWebForms.Pages.Catalog
{
    public class EditModel : PageModel
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(EditModel));

        private readonly ICatalogService _catalogService;

        [BindProperty]
        public CatalogItem? Product { get; set; }

        public IEnumerable<CatalogBrand> Brands { get; private set; } = new List<CatalogBrand>();
        public IEnumerable<CatalogType> Types { get; private set; } = new List<CatalogType>();

        public EditModel(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        public IActionResult OnGet(int id)
        {
            _log.Info($"Now loading... /Catalog/Edit/{id}");
            Product = _catalogService.FindCatalogItem(id);

            if (Product == null)
            {
                return NotFound();
            }

            Brands = _catalogService.GetCatalogBrands();
            Types = _catalogService.GetCatalogTypes();
            return Page();
        }

        public IActionResult OnPost(int id)
        {
            if (!ModelState.IsValid)
            {
                Brands = _catalogService.GetCatalogBrands();
                Types = _catalogService.GetCatalogTypes();
                return Page();
            }

            if (Product != null)
            {
                Product.Id = id;
                _catalogService.UpdateCatalogItem(Product);
            }

            return RedirectToPage("/Index");
        }
    }
}
