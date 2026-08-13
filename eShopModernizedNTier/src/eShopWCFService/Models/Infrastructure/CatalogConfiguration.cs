using System;

namespace eShopWCFService.Models.Infrastructure
{
    public static class CatalogConfiguration
    {
        /// <summary>
        /// Returns the connection string from the environment variable "ConnectionString" if set,
        /// otherwise returns null (Program.cs falls back to appsettings.json / default).
        /// </summary>
        public static string? ConnectionString =>
            Environment.GetEnvironmentVariable("ConnectionString");
    }
}
