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

        public CatalogItem Product { get; private set; } = null!;

        public DetailsModel(ICatalogService catalogService, ILogger<DetailsModel> logger)
        {
            _catalogService = catalogService;
            _logger = logger;
        }

        public IActionResult OnGet(int id)
        {
            _logger.LogInformation("Now loading... /Catalog/Details?id={Id}", id);
            var item = _catalogService.FindCatalogItem(id);
            if (item == null)
            {
                return NotFound();
            }

            Product = item;
            return Page();
        }
    }
}
