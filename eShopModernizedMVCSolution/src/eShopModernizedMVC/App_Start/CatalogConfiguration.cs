namespace eShopModernizedMVC
{
    /// <summary>
    /// Provides catalog configuration values from the DI-injected IConfiguration.
    /// For use in code that cannot easily use constructor injection.
    /// </summary>
    public static class CatalogConfiguration
    {
        private static IConfiguration? _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static bool UseMockData => IsEnabled("UseMockData");
        public static bool UseAzureStorage => IsEnabled("UseAzureStorage");
        public static bool UseManagedIdentity => IsEnabled("UseAzureManagedIdentity");
        public static bool UseCustomizationData => IsEnabled("UseCustomizationData");
        public static bool UseAzureActiveDirectory => IsEnabled("UseAzureActiveDirectory");

        public static string StorageConnectionString =>
            _configuration?["StorageConnectionString"] ?? string.Empty;

        public static string AppInsightsInstrumentationKey =>
            _configuration?["AppInsightsInstrumentationKey"] ?? string.Empty;

        public static string AzureActiveDirectoryClientId =>
            _configuration?["AzureActiveDirectoryClientId"] ?? string.Empty;

        public static string AzureActiveDirectoryTenant =>
            _configuration?["AzureActiveDirectoryTenant"] ?? string.Empty;

        public static string PostLogoutRedirectUri =>
            _configuration?["PostLogoutRedirectUri"] ?? string.Empty;

        private static bool IsEnabled(string configurationKey)
        {
            return _configuration?.GetValue<bool>(configurationKey) ?? false;
        }
    }
}
