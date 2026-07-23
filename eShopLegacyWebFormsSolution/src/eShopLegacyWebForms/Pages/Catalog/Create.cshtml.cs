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
        public CatalogItem CatalogItem { get; set; } = new CatalogItem();

        public SelectList BrandSelectList { get; private set; } = null!;
        public SelectList TypeSelectList { get; private set; } = null!;

        public CreateModel(ICatalogService catalogService, ILogger<CreateModel> logger)
        {
            _catalogService = catalogService;
            _logger = logger;
        }

        public void OnGet()
        {
            _logger.LogInformation("Now loading... /Catalog/Create");
            PopulateDropDowns();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                PopulateDropDowns();
                return Page();
            }

            _catalogService.CreateCatalogItem(CatalogItem);
            return RedirectToPage("/Index");
        }

        private void PopulateDropDowns()
        {
            BrandSelectList = new SelectList(_catalogService.GetCatalogBrands(), "Id", "Brand");
            TypeSelectList = new SelectList(_catalogService.GetCatalogTypes(), "Id", "Type");
        }
    }
}
