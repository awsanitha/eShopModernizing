using Microsoft.Extensions.Configuration;

namespace eShopModernizedMVC
{
    public interface ISqlConnectionFactory
    {
        string GetConnectionString();
    }

    public class ManagedIdentitySqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public ManagedIdentitySqlConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetConnectionString()
        {
            // When using managed identity, Authentication=Active Directory Managed Identity
            // is expected to be part of the configured connection string.
            return _configuration.GetConnectionString("CatalogDBContext");
        }
    }

    public class AppSettingsSqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public AppSettingsSqlConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetConnectionString()
        {
            return _configuration.GetConnectionString("CatalogDBContext");
        }
    }
}
