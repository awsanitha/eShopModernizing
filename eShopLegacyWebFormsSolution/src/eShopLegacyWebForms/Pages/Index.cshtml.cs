using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using eShopLegacyWebForms.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eShopLegacyWebForms.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ICatalogService _catalogService;
        private readonly ILogger<IndexModel> _logger;

        public const int DefaultPageIndex = 0;
        public const int DefaultPageSize = 10;

        public PaginatedItemsViewModel<CatalogItem>? CatalogModel { get; private set; }

        public IndexModel(ICatalogService catalogService, ILogger<IndexModel> logger)
        {
            _catalogService = catalogService;
            _logger = logger;
        }

        public void OnGet([FromQuery] int pageIndex = DefaultPageIndex, [FromQuery] int pageSize = DefaultPageSize)
        {
            _logger.LogInformation("Now loading... /?pageIndex={PageIndex}&pageSize={PageSize}", pageIndex, pageSize);
            CatalogModel = _catalogService.GetCatalogItemsPaginated(pageSize, pageIndex);

            // Set session info for layout display
            var machineName = HttpContext.Session.GetString("MachineName") ?? Environment.MachineName;
            var sessionStart = HttpContext.Session.GetString("SessionStartTime") ?? DateTime.Now.ToString("O");
            ViewData["SessionInfo"] = $"{machineName}, {sessionStart}";
        }
    }
}
