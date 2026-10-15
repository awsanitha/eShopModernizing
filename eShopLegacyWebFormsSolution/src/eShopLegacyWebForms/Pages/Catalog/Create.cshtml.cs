using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace eShopLegacyWebForms.Pages.Catalog
{
    public class CreateModel : PageModel
    {
        private readonly ICatalogService _catalogService;
        private readonly ILogger<CreateModel> _logger;

        [BindProperty]
        public CatalogItem? Item { get; set; }

        public SelectList BrandSelectList { get; private set; } = default!;
        public SelectList TypeSelectList { get; private set; } = default!;

        public CreateModel(ICatalogService catalogService, ILogger<CreateModel> logger)
        {
            _catalogService = catalogService;
            _logger = logger;
        }

        public void OnGet()
        {
            _logger.LogInformation("Now loading... /Catalog/Create");
            PopulateSelectLists();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                PopulateSelectLists();
                return Page();
            }

            if (Item != null)
            {
                _catalogService.CreateCatalogItem(Item);
            }

            return RedirectToPage("/Index");
        }

        private void PopulateSelectLists()
        {
            BrandSelectList = new SelectList(_catalogService.GetCatalogBrands(), "Id", "Brand");
            TypeSelectList = new SelectList(_catalogService.GetCatalogTypes(), "Id", "Type");
        }
    }
}
