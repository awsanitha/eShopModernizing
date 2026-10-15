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
        public CatalogItem? Item { get; set; }

        public SelectList BrandSelectList { get; private set; } = default!;
        public SelectList TypeSelectList { get; private set; } = default!;

        public EditModel(ICatalogService catalogService, ILogger<EditModel> logger)
        {
            _catalogService = catalogService;
            _logger = logger;
        }

        public IActionResult OnGet(int id)
        {
            _logger.LogInformation("Now loading... /Catalog/Edit/{Id}", id);
            Item = _catalogService.FindCatalogItem(id);

            if (Item == null)
            {
                return NotFound();
            }

            PopulateSelectLists();
            return Page();
        }

        public IActionResult OnPost(int id)
        {
            if (!ModelState.IsValid)
            {
                PopulateSelectLists();
                return Page();
            }

            if (Item != null)
            {
                Item.Id = id;
                _catalogService.UpdateCatalogItem(Item);
            }

            return RedirectToPage("/Index");
        }

        private void PopulateSelectLists()
        {
            BrandSelectList = new SelectList(_catalogService.GetCatalogBrands(), "Id", "Brand", Item?.CatalogBrandId);
            TypeSelectList = new SelectList(_catalogService.GetCatalogTypes(), "Id", "Type", Item?.CatalogTypeId);
        }
    }
}
