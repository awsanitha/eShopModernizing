using Microsoft.Extensions.Configuration;

namespace eShopModernizedWebForms
{
    public static class CatalogConfiguration
    {
        private static IConfiguration _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static bool UseMockData => IsEnabled("UseMockData");

        public static bool UseManagedIdentity => IsEnabled("UseAzureManagedIdentity");

        public static bool UseAzureStorage => IsEnabled("UseAzureStorage");

        public static bool UseCustomizationData => IsEnabled("UseCustomizationData");

        public static string StorageConnectionString => _configuration["StorageConnectionString"];

        public static string AppInsightsInstrumentationKey => _configuration["AppInsightsInstrumentationKey"];

        public static bool UseAzureActiveDirectory => IsEnabled("UseAzureActiveDirectory");

        public static string AzureActiveDirectoryClientId => _configuration["AzureActiveDirectoryClientId"];

        public static string AzureActiveDirectoryTenant => _configuration["AzureActiveDirectoryTenant"];

        public static string AzureActiveDirectoryInstance => _configuration["AzureActiveDirectoryInstance"];

        public static string PostLogoutRedirectUri => _configuration["PostLogoutRedirectUri"];

        private static bool IsEnabled(string configurationKey)
        {
            return _configuration.GetValue<bool>(configurationKey);
        }
    }
}
