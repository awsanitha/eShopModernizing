using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace eShopLegacyWebForms.Pages.Catalog
{
    public class CreateModel : PageModel
    {
        private readonly ICatalogService _catalogService;
        private readonly ILogger<CreateModel> _logger;

        [BindProperty]
        [Required(ErrorMessage = "The Name field is required.")]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        public string? Description { get; set; }

        [BindProperty]
        [Required]
        public int BrandId { get; set; }

        [BindProperty]
        [Required]
        public int TypeId { get; set; }

        [BindProperty]
        [Range(0, 1000000, ErrorMessage = "The Price must be a positive number with maximum two decimals between 0 and 1 million.")]
        public decimal Price { get; set; }

        [BindProperty]
        [Range(0, 10000000, ErrorMessage = "The field Stock must be between 0 and 10 million.")]
        public int Stock { get; set; }

        [BindProperty]
        [Range(0, 10000000, ErrorMessage = "The field Restock must be between 0 and 10 million.")]
        public int Restock { get; set; }

        [BindProperty]
        [Range(0, 10000000, ErrorMessage = "The field Max stock must be between 0 and 10 million.")]
        public int Maxstock { get; set; }

        public IEnumerable<SelectListItem> Brands { get; private set; } = [];
        public IEnumerable<SelectListItem> Types { get; private set; } = [];

        public CreateModel(ICatalogService catalogService, ILogger<CreateModel> logger)
        {
            _catalogService = catalogService;
            _logger = logger;
        }

        public void OnGet()
        {
            _logger.LogInformation("Now loading... /Catalog/Create");
            LoadSelectLists();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                LoadSelectLists();
                return Page();
            }

            var catalogItem = new CatalogItem
            {
                Name = Name,
                Description = Description ?? string.Empty,
                CatalogBrandId = BrandId,
                CatalogTypeId = TypeId,
                Price = Price,
                AvailableStock = Stock,
                RestockThreshold = Restock,
                MaxStockThreshold = Maxstock
            };

            _catalogService.CreateCatalogItem(catalogItem);

            return RedirectToPage("/Index");
        }

        private void LoadSelectLists()
        {
            Brands = _catalogService.GetCatalogBrands()
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Brand });
            Types = _catalogService.GetCatalogTypes()
                .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Type });
        }
    }
}
