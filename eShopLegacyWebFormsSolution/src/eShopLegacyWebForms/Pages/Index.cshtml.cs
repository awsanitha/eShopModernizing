using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using eShopLegacyWebForms.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace eShopLegacyWebForms.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ICatalogService _catalogService;
        private readonly ILogger<IndexModel> _logger;

        public const int DefaultPageIndex = 0;
        public const int DefaultPageSize = 10;

        public PaginatedItemsViewModel<CatalogItem> CatalogModel { get; private set; } = null!;

        public IndexModel(ICatalogService catalogService, ILogger<IndexModel> logger)
        {
            _catalogService = catalogService;
            _logger = logger;
        }

        public void OnGet(int? index, int? size)
        {
            // Store machine/session info (equivalent to old Session_Start)
            if (HttpContext.Session.GetString("MachineName") == null)
            {
                HttpContext.Session.SetString("MachineName", Environment.MachineName);
                HttpContext.Session.SetString("SessionStartTime", DateTime.Now.ToString());
            }

            var pageIndex = index ?? DefaultPageIndex;
            var pageSize = size ?? DefaultPageSize;

            CatalogModel = _catalogService.GetCatalogItemsPaginated(pageSize, pageIndex);
            _logger.LogInformation("Now loading... /Default?size={Size}&index={Index}", pageSize, pageIndex);
        }
    }
}
