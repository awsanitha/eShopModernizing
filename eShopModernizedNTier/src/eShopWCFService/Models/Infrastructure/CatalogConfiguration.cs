using System;

namespace eShopWCFService.Models.Infrastructure
{
    public class CatalogConfiguration
    {
        public static string ConnectionString
        {
            get
            {
                var envConnectionString = Environment.GetEnvironmentVariable("ConnectionString");
                return envConnectionString ?? "Server=(localdb)\\mssqllocaldb;Database=eShopCatalog;Trusted_Connection=True;MultipleActiveResultSets=true";
            }
        }
    }
}
