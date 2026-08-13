using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace eShopModernizedWebForms
{
    public interface ISqlConnectionFactory
    {
        SqlConnection CreateConnection();
    }

    public class ManagedIdentitySqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public ManagedIdentitySqlConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection { ConnectionString = _configuration.GetConnectionString("CatalogDBContext") };
        }
    }

    public class AppSettingsSqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public AppSettingsSqlConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public SqlConnection CreateConnection()
        {
            return new SqlConnection { ConnectionString = _configuration.GetConnectionString("CatalogDBContext") };
        }
    }
}
