using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace eShopLegacyWebForms.Pages.Catalog
{
    public class CreateModel : PageModel
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(CreateModel));

        private readonly ICatalogService _catalogService;

        [BindProperty]
        public CatalogItem CatalogItem { get; set; } = new CatalogItem();

        public IEnumerable<CatalogBrand> Brands { get; private set; } = new List<CatalogBrand>();
        public IEnumerable<CatalogType> Types { get; private set; } = new List<CatalogType>();

        public CreateModel(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        public void OnGet()
        {
            _log.Info("Now loading... /Catalog/Create");
            Brands = _catalogService.GetCatalogBrands();
            Types = _catalogService.GetCatalogTypes();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                Brands = _catalogService.GetCatalogBrands();
                Types = _catalogService.GetCatalogTypes();
                return Page();
            }

            _catalogService.CreateCatalogItem(CatalogItem);
            return RedirectToPage("/Index");
        }
    }
}
