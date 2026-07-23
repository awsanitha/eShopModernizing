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

        public IEnumerable<CatalogItem> CatalogItems { get; private set; } = Enumerable.Empty<CatalogItem>();
        public PaginatedItemsViewModel<CatalogItem> Pagination { get; private set; } = null!;

        public IndexModel(ICatalogService catalogService, ILogger<IndexModel> logger)
        {
            _catalogService = catalogService;
            _logger = logger;
        }

        public void OnGet(int? index, int? size)
        {
            // Set session info (mimicking the old Session_Start behavior)
            if (HttpContext.Session.GetString("MachineName") == null)
            {
                HttpContext.Session.SetString("MachineName", Environment.MachineName);
                HttpContext.Session.SetString("SessionStartTime", DateTime.Now.ToString());
            }

            var pageIndex = index ?? DefaultPageIndex;
            var pageSize = size ?? DefaultPageSize;

            _logger.LogInformation("Now loading... /Index?size={Size}&index={Index}", pageSize, pageIndex);

            Pagination = _catalogService.GetCatalogItemsPaginated(pageSize, pageIndex);
            CatalogItems = Pagination.Data;
        }
    }
}
