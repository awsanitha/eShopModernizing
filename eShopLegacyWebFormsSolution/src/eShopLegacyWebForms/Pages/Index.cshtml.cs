using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Services;
using eShopLegacyWebForms.ViewModel;
using log4net;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eShopLegacyWebForms.Pages
{
    public class IndexModel : PageModel
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(IndexModel));

        private readonly ICatalogService _catalogService;

        public const int DefaultPageIndex = 0;
        public const int DefaultPageSize = 10;

        public PaginatedItemsViewModel<CatalogItem>? CatalogModel { get; private set; }

        public IndexModel(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        public void OnGet(int pageIndex = DefaultPageIndex, int pageSize = DefaultPageSize)
        {
            // Set session info for display
            if (HttpContext.Session.GetString("MachineName") == null)
            {
                HttpContext.Session.SetString("MachineName", Environment.MachineName);
                HttpContext.Session.SetString("SessionStartTime", DateTime.Now.ToString());
            }
            ViewData["SessionInfo"] = $"{HttpContext.Session.GetString("MachineName")}, {HttpContext.Session.GetString("SessionStartTime")}";

            CatalogModel = _catalogService.GetCatalogItemsPaginated(pageSize, pageIndex);
            _log.Info($"Now loading... /Index?pageIndex={pageIndex}&pageSize={pageSize}");
        }
    }
}
