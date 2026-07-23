using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using eShopLegacyWebForms.ViewModel;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eShopLegacyWebForms.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ICatalogService _catalogService;
        private readonly ILogger<IndexModel> _logger;

        public const int DefaultPageIndex = 0;
        public const int DefaultPageSize = 10;

        public PaginatedItemsViewModel<CatalogItem> PaginatedItems { get; private set; } = null!;
        public IEnumerable<CatalogItem> CatalogItems => PaginatedItems.Data;

        public IndexModel(ICatalogService catalogService, ILogger<IndexModel> logger)
        {
            _catalogService = catalogService;
            _logger = logger;
        }

        public void OnGet(int pageIndex = DefaultPageIndex, int pageSize = DefaultPageSize)
        {
            _logger.LogInformation("Now loading... /Index?size={PageSize}&index={PageIndex}", pageSize, pageIndex);
            PaginatedItems = _catalogService.GetCatalogItemsPaginated(pageSize, pageIndex);
        }
    }
}
