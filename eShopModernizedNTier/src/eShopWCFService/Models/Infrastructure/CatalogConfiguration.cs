using System;

namespace eShopWCFService.Models.Infrastructure
{
    public class CatalogConfiguration
    {
        private const string DefaultConnectionString =
            "Server=(localdb)\\mssqllocaldb;Database=eShopDatabase;Persist Security Info=True;MultipleActiveResultSets=True";

        public static string ConnectionString
        {
            get
            {
                var envConnectionString = Environment.GetEnvironmentVariable("ConnectionString");
                return envConnectionString ?? DefaultConnectionString;
            }
        }
    }
}
