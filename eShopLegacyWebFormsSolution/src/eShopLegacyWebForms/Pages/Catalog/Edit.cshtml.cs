using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace eShopLegacyWebForms.Pages.Catalog
{
    public class EditModel : PageModel
    {
        private readonly ICatalogService _catalogService;
        private readonly ILogger<EditModel> _logger;

        [BindProperty]
        public CatalogItem? CatalogItem { get; set; }

        public SelectList BrandSelectList { get; private set; } = null!;
        public SelectList TypeSelectList { get; private set; } = null!;

        public EditModel(ICatalogService catalogService, ILogger<EditModel> logger)
        {
            _catalogService = catalogService;
            _logger = logger;
        }

        public IActionResult OnGet(int id)
        {
            _logger.LogInformation("Now loading... /Catalog/Edit?id={Id}", id);

            CatalogItem = _catalogService.FindCatalogItem(id);
            if (CatalogItem == null)
            {
                return NotFound();
            }

            PopulateDropDowns(CatalogItem.CatalogBrandId, CatalogItem.CatalogTypeId);
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid || CatalogItem == null)
            {
                PopulateDropDowns(CatalogItem?.CatalogBrandId ?? 0, CatalogItem?.CatalogTypeId ?? 0);
                return Page();
            }

            _catalogService.UpdateCatalogItem(CatalogItem);
            return RedirectToPage("/Index");
        }

        private void PopulateDropDowns(int selectedBrandId = 0, int selectedTypeId = 0)
        {
            BrandSelectList = new SelectList(_catalogService.GetCatalogBrands(), "Id", "Brand", selectedBrandId);
            TypeSelectList = new SelectList(_catalogService.GetCatalogTypes(), "Id", "Type", selectedTypeId);
        }
    }
}
