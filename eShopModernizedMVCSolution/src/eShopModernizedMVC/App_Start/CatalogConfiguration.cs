using Microsoft.Extensions.Configuration;

namespace eShopModernizedMVC
{
    public class CatalogConfiguration
    {
        private static IConfiguration _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static bool UseMockData
        {
            get => IsEnabled("UseMockData");
        }

        public static bool UseAzureStorage
        {
            get => IsEnabled("UseAzureStorage");
        }

        public static bool UseManagedIdentity
        {
            get => IsEnabled("UseAzureManagedIdentity");
        }

        public static bool UseCustomizationData
        {
            get => IsEnabled("UseCustomizationData");
        }

        public static string StorageConnectionString
        {
            get => _configuration?["StorageConnectionString"] ?? string.Empty;
        }

        public static string AppInsightsInstrumentationKey
        {
            get => _configuration?["AppInsightsInstrumentationKey"] ?? string.Empty;
        }

        public static bool UseAzureActiveDirectory
        {
            get => IsEnabled("UseAzureActiveDirectory");
        }

        public static string AzureActiveDirectoryClientId
        {
            get => _configuration?["AzureActiveDirectoryClientId"] ?? string.Empty;
        }

        public static string AzureActiveDirectoryTenant
        {
            get => _configuration?["AzureActiveDirectoryTenant"] ?? string.Empty;
        }

        public static string PostLogoutRedirectUri
        {
            get => _configuration?["PostLogoutRedirectUri"] ?? string.Empty;
        }

        private static bool IsEnabled(string configurationKey)
        {
            var value = _configuration?[configurationKey];
            return bool.TryParse(value, out bool result) && result;
        }
    }
}
