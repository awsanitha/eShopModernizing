using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eShopLegacyWebForms.Pages.Catalog
{
    public class DeleteModel : PageModel
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly ICatalogService _catalogService;

        [BindProperty]
        public CatalogItem? ProductToDelete { get; set; }

        public DeleteModel(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        public IActionResult OnGet(int id)
        {
            _log.Info($"Now loading... /Catalog/Delete/{id}");
            ProductToDelete = _catalogService.FindCatalogItem(id);

            if (ProductToDelete == null)
            {
                return NotFound();
            }

            return Page();
        }

        public IActionResult OnPost(int id)
        {
            var productToDelete = _catalogService.FindCatalogItem(id);

            if (productToDelete != null)
            {
                _catalogService.RemoveCatalogItem(productToDelete);
            }

            return RedirectToPage("/Index");
        }
    }
}
