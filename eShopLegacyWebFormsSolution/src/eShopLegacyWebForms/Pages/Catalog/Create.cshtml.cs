using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eShopLegacyWebForms.Pages.Catalog
{
    public class CreateModel : PageModel
    {
        private readonly ICatalogService _catalogService;
        private readonly ILogger<CreateModel> _logger;

        [BindProperty]
        public CatalogItem CatalogItem { get; set; } = new CatalogItem();

        public IEnumerable<CatalogBrand> Brands { get; private set; } = [];
        public IEnumerable<CatalogType> Types { get; private set; } = [];

        public CreateModel(ICatalogService catalogService, ILogger<CreateModel> logger)
        {
            _catalogService = catalogService;
            _logger = logger;
        }

        public void OnGet()
        {
            _logger.LogInformation("Now loading... /Catalog/Create");
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
