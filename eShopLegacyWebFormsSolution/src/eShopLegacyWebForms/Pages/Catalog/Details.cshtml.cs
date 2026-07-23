using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eShopLegacyWebForms.Pages.Catalog
{
    public class DetailsModel : PageModel
    {
        private readonly ICatalogService _catalogService;
        private readonly ILogger<DetailsModel> _logger;

        public CatalogItem? CatalogItem { get; private set; }

        public DetailsModel(ICatalogService catalogService, ILogger<DetailsModel> logger)
        {
            _catalogService = catalogService;
            _logger = logger;
        }

        public IActionResult OnGet(int id)
        {
            _logger.LogInformation("Now loading... /Catalog/Details?id={Id}", id);

            CatalogItem = _catalogService.FindCatalogItem(id);
            if (CatalogItem == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
