using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace eShopLegacyWebForms.Pages.Catalog
{
    public class DeleteModel : PageModel
    {
        private readonly ICatalogService _catalogService;
        private readonly ILogger<DeleteModel> _logger;

        public CatalogItem? ProductToDelete { get; private set; }

        public DeleteModel(ICatalogService catalogService, ILogger<DeleteModel> logger)
        {
            _catalogService = catalogService;
            _logger = logger;
        }

        public IActionResult OnGet(int id)
        {
            _logger.LogInformation("Now loading... /Catalog/Delete?id={Id}", id);
            ProductToDelete = _catalogService.FindCatalogItem(id);

            if (ProductToDelete == null)
            {
                return NotFound();
            }

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
