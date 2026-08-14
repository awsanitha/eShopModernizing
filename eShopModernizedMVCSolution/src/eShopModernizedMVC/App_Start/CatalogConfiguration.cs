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
            get
            {
                return IsEnabled("UseMockData");
            }
        }

        public static bool UseAzureStorage
        {
            get
            {
                return IsEnabled("UseAzureStorage");
            }
        }

        public static bool UseManagedIdentity
        {
            get
            {
                return IsEnabled("UseAzureManagedIdentity");
            }
        }

        public static bool UseCustomizationData
        {
            get
            {
                return IsEnabled("UseCustomizationData");
            }
        }

        public static string StorageConnectionString
        {
            get
            {
                return _configuration["StorageConnectionString"];
            }
        }

        public static string AppInsightsInstrumentationKey
        {
            get
            {
                return _configuration["AppInsightsInstrumentationKey"];
            }
        }

        public static bool UseAzureActiveDirectory
        {
            get
            {
                return IsEnabled("UseAzureActiveDirectory");
            }
        }

        public static string AzureActiveDirectoryClientId
        {
            get
            {
                return _configuration["AzureActiveDirectoryClientId"];
            }
        }

        public static string AzureActiveDirectoryTenant
        {
            get
            {
                return _configuration["AzureActiveDirectoryTenant"];
            }
        }

        public static string AzureActiveDirectoryInstance
        {
            get
            {
                return _configuration["AzureActiveDirectoryInstance"];
            }
        }

        public static string PostLogoutRedirectUri
        {
            get
            {
                return _configuration["PostLogoutRedirectUri"];
            }
        }

        private static bool IsEnabled(string configurationKey)
        {
            return bool.Parse(_configuration[configurationKey] ?? "false");
        }
    }
}
