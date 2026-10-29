using eShopWCFService;
using eShopWCFService.Models.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eShopWCFService.Models.Infrastructure
{
    /// <summary>
    /// Creates the catalog database if it does not already exist and seeds it with the
    /// preconfigured data. Replaces the Entity Framework 6 CreateDatabaseIfNotExists
    /// initializer, which has no EF Core equivalent.
    /// </summary>
    public class CatalogDBInitializer
    {
        private static readonly SemaphoreSlim initializationGate = new SemaphoreSlim(1, 1);
        private static bool initialized;

        /// <summary>
        /// Runs <see cref="InitializeDatabase"/> at most once per process, and only latches as
        /// done when it actually succeeds. The EF6 initializer was registered per context and
        /// ran on first database access, so a database that was unavailable at startup would
        /// still be created and seeded on a later call. Keeping the attempt retryable
        /// preserves that behaviour instead of leaving the service permanently pointed at a
        /// missing database.
        /// </summary>
        public static void EnsureInitialized(EntityModel context, ILogger logger = null)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (Volatile.Read(ref initialized))
            {
                return;
            }

            initializationGate.Wait();
            try
            {
                if (Volatile.Read(ref initialized))
                {
                    return;
                }

                new CatalogDBInitializer().InitializeDatabase(context);
                Volatile.Write(ref initialized, true);
            }
            catch (Exception ex)
            {
                // Deliberately not latched: the next operation retries, matching EF6's
                // initialize-on-first-access behaviour.
                logger?.LogCritical(ex, "Could not create or seed the catalog database. It will be retried on the next operation.");
            }
            finally
            {
                initializationGate.Release();
            }
        }

        /// <summary>
        /// Creates the database when missing and seeds it. Seeding only runs for a freshly
        /// created database, preserving the CreateDatabaseIfNotExists semantics.
        /// </summary>
        public void InitializeDatabase(EntityModel context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var created = context.Database.EnsureCreated();
            if (created)
            {
                Seed(context);
            }
        }

        protected virtual void Seed(EntityModel context)
        {
            AddCatalogTypes(context);
            AddCatalogBrands(context);
            AddCatalogItems(context);
            AddCatalogItemsStock(context);
            AddDiscountItems(context);
        }

        private void AddCatalogTypes(EntityModel context)
        {
            var preconfiguredTypes = PreconfiguredData.GetPreconfiguredCatalogTypes();

            foreach (var type in preconfiguredTypes)
            {
                context.CatalogTypes.Add(type);
            }

            context.SaveChanges();
        }

        private void AddCatalogBrands(EntityModel context)
        {
            var preconfiguredBrands = PreconfiguredData.GetPreconfiguredCatalogBrands();

            foreach (var brand in preconfiguredBrands)
            {
                context.CatalogBrands.Add(brand);
            }

            context.SaveChanges();
        }

        private void AddDiscountItems(EntityModel context)
        {
            var preconfiguredDiscounts = PreconfiguredData.GetPreconfiguredDiscountItems();

            foreach (var discount in preconfiguredDiscounts)
            {
                context.DiscountItems.Add(discount);
            }

            context.SaveChanges();
        }

        private void AddCatalogItems(EntityModel context)
        {
            var preconfiguredItems = PreconfiguredData.GetPreconfiguredCatalogItems();

            foreach (var item in preconfiguredItems)
            {
                context.CatalogItems.Add(item);
            }

            context.SaveChanges();
        }

        private void AddCatalogItemsStock(EntityModel context)
        {
            var preconfiguredStock = PreconfiguredData.GetPreconfiguredCatalogItemsStock();

            foreach (var s in preconfiguredStock)
            {
                context.CatalogItemsStocks.Add(s);
            }

            context.SaveChanges();
        }
    }
}
