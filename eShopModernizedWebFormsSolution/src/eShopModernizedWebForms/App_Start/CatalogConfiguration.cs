using Microsoft.Extensions.Configuration;

namespace eShopModernizedWebForms
{
    public class CatalogConfiguration
    {
        private readonly IConfiguration _configuration;

        public CatalogConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool UseMockData => _configuration.GetValue<bool>("UseMockData");
        public bool UseManagedIdentity => _configuration.GetValue<bool>("UseAzureManagedIdentity");
        public bool UseAzureStorage => _configuration.GetValue<bool>("UseAzureStorage");
        public bool UseCustomizationData => _configuration.GetValue<bool>("UseCustomizationData");
        public string StorageConnectionString => _configuration["StorageConnectionString"] ?? "";
        public string AppInsightsInstrumentationKey => _configuration["AppInsightsInstrumentationKey"] ?? "";
    }
}
