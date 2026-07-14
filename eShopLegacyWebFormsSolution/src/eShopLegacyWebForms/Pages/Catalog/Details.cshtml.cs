using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eShopLegacyWebForms.Pages.Catalog
{
    public class DetailsModel : PageModel
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly ICatalogService _catalogService;

        public CatalogItem? Product { get; private set; }

        public DetailsModel(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        public IActionResult OnGet(int id)
        {
            _log.Info($"Now loading... /Catalog/Details/{id}");
            Product = _catalogService.FindCatalogItem(id);

            if (Product == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
