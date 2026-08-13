using Azure.Identity;
using Microsoft.Data.SqlClient;

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
            var credential = new DefaultAzureCredential();
            var tokenRequestContext = new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" });
            var token = credential.GetToken(tokenRequestContext);

            var connectionString = _configuration.GetConnectionString("CatalogDBContext")
                ?? throw new InvalidOperationException("CatalogDBContext connection string not found.");

            return new SqlConnection
            {
                ConnectionString = connectionString,
                AccessToken = token.Token
            };
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
            var connectionString = _configuration.GetConnectionString("CatalogDBContext")
                ?? throw new InvalidOperationException("CatalogDBContext connection string not found.");

            return new SqlConnection { ConnectionString = connectionString };
        }
    }
}
