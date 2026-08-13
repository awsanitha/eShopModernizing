using Microsoft.Extensions.Configuration;

namespace eShopModernizedWebForms
{
    // In ASP.NET Core, managed identity connection is handled via connection string configuration
    // or Azure.Identity. This class is kept for structural compatibility.
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
            return _configuration.GetConnectionString("DefaultConnection") ?? "";
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
            return _configuration.GetConnectionString("DefaultConnection") ?? "";
        }
    }
}
