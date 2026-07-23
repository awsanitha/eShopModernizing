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

        public CatalogItem CatalogItem { get; private set; } = null!;

        public DeleteModel(ICatalogService catalogService, ILogger<DeleteModel> logger)
        {
            _catalogService = catalogService;
            _logger = logger;
        }

        public IActionResult OnGet(int id)
        {
            _logger.LogInformation("Now loading... /Catalog/Delete?id={ProductId}", id);

            var item = _catalogService.FindCatalogItem(id);
            if (item == null)
            {
                return NotFound();
            }

            CatalogItem = item;
            return Page();
        }

        public IActionResult OnPost(int id)
        {
            var item = _catalogService.FindCatalogItem(id);
            if (item != null)
            {
                _catalogService.RemoveCatalogItem(item);
            }

            return RedirectToPage("/Index");
        }
    }
}
