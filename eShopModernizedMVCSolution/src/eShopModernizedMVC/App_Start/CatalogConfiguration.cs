using Microsoft.Extensions.Configuration;

namespace eShopModernizedMVC
{
    public static class CatalogConfiguration
    {
        private static IConfiguration? _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static bool UseMockData => GetBool("UseMockData");
        public static bool UseAzureStorage => GetBool("UseAzureStorage");
        public static bool UseManagedIdentity => GetBool("UseManagedIdentity");
        public static bool UseCustomizationData => GetBool("UseCustomizationData");
        public static string StorageConnectionString => _configuration?["StorageConnectionString"] ?? "";
        public static string AppInsightsInstrumentationKey => _configuration?["AppInsightsInstrumentationKey"] ?? "";
        public static bool UseAzureActiveDirectory => GetBool("UseAzureActiveDirectory");
        public static string AzureActiveDirectoryClientId => _configuration?["AzureActiveDirectoryClientId"] ?? "";
        public static string AzureActiveDirectoryTenant => _configuration?["AzureActiveDirectoryTenant"] ?? "";
        public static string PostLogoutRedirectUri => _configuration?["PostLogoutRedirectUri"] ?? "";

        private static bool GetBool(string key)
        {
            var value = _configuration?[key];
            return bool.TryParse(value, out var result) && result;
        }
    }
}
