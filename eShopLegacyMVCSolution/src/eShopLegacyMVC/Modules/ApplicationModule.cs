using eShopLegacyMVC.Models;
using eShopLegacyMVC.Models.Infrastructure;
using eShopLegacyMVC.Services;

namespace eShopLegacyMVC.Modules
{
    /// <summary>
    /// Application module that defines service registrations.
    /// This is retained as a configuration class for documentation purposes.
    /// Actual DI registration is done in Program.cs using Microsoft.Extensions.DependencyInjection.
    /// </summary>
    public static class ApplicationModule
    {
        // Service registrations are handled directly in Program.cs:
        // - ICatalogService -> CatalogServiceMock (singleton, when UseMockData = true)
        // - ICatalogService -> CatalogService (scoped, when UseMockData = false)
        // - CatalogDBContext (scoped, via AddDbContext)
        // - CatalogDBInitializer (scoped)
        // - CatalogItemHiLoGenerator (singleton)
    }
}
