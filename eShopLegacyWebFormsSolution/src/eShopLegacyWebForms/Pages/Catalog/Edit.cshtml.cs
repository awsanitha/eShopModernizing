using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eShopLegacyWebForms.Pages.Catalog
{
    public class EditModel : PageModel
    {
        private readonly ICatalogService _catalogService;
        private readonly ILogger<EditModel> _logger;

        [BindProperty]
        public CatalogItem CatalogItem { get; set; } = null!;

        public IEnumerable<CatalogBrand> Brands { get; private set; } = Enumerable.Empty<CatalogBrand>();
        public IEnumerable<CatalogType> Types { get; private set; } = Enumerable.Empty<CatalogType>();

        public EditModel(ICatalogService catalogService, ILogger<EditModel> logger)
        {
            _catalogService = catalogService;
            _logger = logger;
        }

        public IActionResult OnGet(int id)
        {
            _logger.LogInformation("Now loading... /Catalog/Edit?id={Id}", id);
            var item = _catalogService.FindCatalogItem(id);
            if (item == null)
            {
                return NotFound();
            }

            CatalogItem = item;
            Brands = _catalogService.GetCatalogBrands();
            Types = _catalogService.GetCatalogTypes();
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                Brands = _catalogService.GetCatalogBrands();
                Types = _catalogService.GetCatalogTypes();
                return Page();
            }

            _catalogService.UpdateCatalogItem(CatalogItem);
            return RedirectToPage("/Index");
        }
    }
}
