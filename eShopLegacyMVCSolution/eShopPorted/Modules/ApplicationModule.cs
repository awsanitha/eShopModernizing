namespace eShopPorted.Modules
{
    /// <summary>
    /// Application module - kept as a namespace placeholder.
    /// Service registrations are now handled directly in Program.cs
    /// using Microsoft.Extensions.DependencyInjection.
    /// </summary>
    public static class ApplicationModule
    {
        // ICatalogService is registered in Program.cs:
        // - CatalogServiceMock (singleton) when UseMockData = true
        // - CatalogService (scoped) when UseMockData = false
    }
}
