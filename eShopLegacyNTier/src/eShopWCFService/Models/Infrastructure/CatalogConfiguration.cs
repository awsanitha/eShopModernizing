using System;

namespace eShopWCFService.Models.Infrastructure
{
    public class CatalogConfiguration
    {
        public static string ConnectionString
        {
            get
            {
                // Allow overriding the connection string via environment variable
                var envConnectionString = Environment.GetEnvironmentVariable("ConnectionString");
                return envConnectionString
                    ?? "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=eShopDatabase;Persist Security Info=True;";
            }
        }
    }
}
