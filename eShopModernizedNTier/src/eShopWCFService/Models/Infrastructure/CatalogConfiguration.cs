using System;

namespace eShopWCFService.Models.Infrastructure
{
    public class CatalogConfiguration
    {
        private static readonly string configConnectionName = "EntityModel";

        /// <summary>
        /// Returns the connection string from environment variable, or the config key name
        /// so it can be resolved from appsettings.json via IConfiguration.
        /// </summary>
        public static string ConnectionStringKey
        {
            get
            {
                return configConnectionName;
            }
        }

        /// <summary>
        /// Returns a direct connection string if provided via environment variable,
        /// otherwise null (in which case the app should use IConfiguration).
        /// </summary>
        public static string? EnvironmentConnectionString
        {
            get
            {
                return Environment.GetEnvironmentVariable("ConnectionString");
            }
        }
    }
}
