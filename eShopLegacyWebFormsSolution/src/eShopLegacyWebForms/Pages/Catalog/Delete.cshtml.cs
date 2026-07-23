using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eShopLegacyWebForms.Pages.Catalog
{
    public class DeleteModel : PageModel
    {
        private readonly ICatalogService _catalogService;
        private readonly ILogger<DeleteModel> _logger;

        public CatalogItem? CatalogItem { get; private set; }

        public DeleteModel(ICatalogService catalogService, ILogger<DeleteModel> logger)
        {
            _catalogService = catalogService;
            _logger = logger;
        }

        public IActionResult OnGet(int id)
        {
            _logger.LogInformation("Now loading... /Catalog/Delete?id={Id}", id);

            CatalogItem = _catalogService.FindCatalogItem(id);
            if (CatalogItem == null)
            {
                return NotFound();
            }

            return Page();
        }

        public IActionResult OnPost(int id)
        {
            var catalogItem = _catalogService.FindCatalogItem(id);
            if (catalogItem != null)
            {
                _catalogService.RemoveCatalogItem(catalogItem);
            }

            return RedirectToPage("/Index");
        }
    }
}
