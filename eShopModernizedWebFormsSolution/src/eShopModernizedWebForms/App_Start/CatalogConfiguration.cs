using Microsoft.Extensions.Configuration;

namespace eShopModernizedWebForms
{
    public class CatalogConfiguration
    {
        private static IConfiguration _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static bool UseMockData => IsEnabled("UseMockData");
        public static bool UseAzureStorage => IsEnabled("UseAzureStorage");
        public static bool UseManagedIdentity => IsEnabled("UseAzureManagedIdentity");
        public static bool UseCustomizationData => IsEnabled("UseCustomizationData");
        public static bool UseAzureActiveDirectory => IsEnabled("UseAzureActiveDirectory");
        public static string StorageConnectionString => _configuration?["StorageConnectionString"] ?? string.Empty;
        public static string AppInsightsInstrumentationKey => _configuration?["AppInsightsInstrumentationKey"] ?? string.Empty;
        public static string AzureActiveDirectoryClientId => _configuration?["AzureActiveDirectoryClientId"] ?? string.Empty;
        public static string AzureActiveDirectoryTenant => _configuration?["AzureActiveDirectoryTenant"] ?? string.Empty;
        public static string PostLogoutRedirectUri => _configuration?["PostLogoutRedirectUri"] ?? string.Empty;

        private static bool IsEnabled(string configurationKey)
        {
            var value = _configuration?[configurationKey];
            return bool.TryParse(value, out bool result) && result;
        }
    }
}
