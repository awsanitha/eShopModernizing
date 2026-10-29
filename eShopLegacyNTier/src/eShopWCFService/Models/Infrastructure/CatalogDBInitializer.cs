using eShopWCFService;
using eShopWCFService.Models.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace eShopWCFService.Models.Infrastructure
{
    /// <summary>
    /// Creates the catalog database if it does not already exist and seeds it with the
    /// preconfigured data. Replaces the Entity Framework 6 CreateDatabaseIfNotExists
    /// initializer, which has no EF Core equivalent.
    /// </summary>
    public class CatalogDBInitializer
    {
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
                // The preconfigured items carry only foreign keys; clearing the navigation
                // properties stops EF Core from attempting to insert duplicate brands/types.
                item.CatalogBrand = null;
                item.CatalogType = null;
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
