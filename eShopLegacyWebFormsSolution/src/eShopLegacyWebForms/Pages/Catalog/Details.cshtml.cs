using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace eShopLegacyWebForms.Pages.Catalog
{
    public class DetailsModel : PageModel
    {
        private readonly ICatalogService _catalogService;
        private readonly ILogger<DetailsModel> _logger;

        public CatalogItem? Product { get; private set; }

        public DetailsModel(ICatalogService catalogService, ILogger<DetailsModel> logger)
        {
            _catalogService = catalogService;
            _logger = logger;
        }

        public IActionResult OnGet(int id)
        {
            _logger.LogInformation("Now loading... /Catalog/Details?id={Id}", id);
            Product = _catalogService.FindCatalogItem(id);

            if (Product == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
