using System;

namespace eShopWCFService.Models.Infrastructure
{
    public class CatalogConfiguration
    {
        private const string DefaultConnectionStringName = "EntityModel";

        /// <summary>
        /// Returns the connection string for the catalog database.
        /// Checks the "ConnectionString" environment variable first; falls back to the
        /// named connection string from appsettings.json.
        /// </summary>
        public static string ConnectionStringName => DefaultConnectionStringName;

        public static string? EnvironmentOverride =>
            Environment.GetEnvironmentVariable("ConnectionString");
    }
}
