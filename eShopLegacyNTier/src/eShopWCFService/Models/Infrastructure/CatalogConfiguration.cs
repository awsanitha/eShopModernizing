using System;
using Microsoft.Extensions.Configuration;

namespace eShopWCFService.Models.Infrastructure
{
    public class CatalogConfiguration
    {
        private static readonly string configConnectionName = "EntityModel";

        private static string configuredConnectionString;

        /// <summary>
        /// Supplies the application's <see cref="IConfiguration"/> so that the connection string
        /// can be resolved from appsettings.json, replacing the legacy &lt;connectionStrings&gt; lookup.
        /// </summary>
        public static void Initialize(IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            configuredConnectionString = configuration.GetConnectionString(configConnectionName);
        }

        public static string ConnectionString
        {
            get
            {
                var envConnectionString = Environment.GetEnvironmentVariable("ConnectionString");
                if (!string.IsNullOrEmpty(envConnectionString))
                {
                    return envConnectionString;
                }

                if (string.IsNullOrEmpty(configuredConnectionString))
                {
                    throw new InvalidOperationException(
                        $"Connection string '{configConnectionName}' was not found. Set it in appsettings.json " +
                        "or provide the 'ConnectionString' environment variable.");
                }

                return configuredConnectionString;
            }
        }
    }
}
