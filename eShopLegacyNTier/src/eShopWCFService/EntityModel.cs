using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using eShopWCFService.Models;
using eShopWCFService.Models.Infrastructure;

namespace eShopWCFService
{
    public partial class EntityModel : DbContext
    {
        public EntityModel(DbContextOptions<EntityModel> options)
            : base(options)
        {
        }

        public virtual DbSet<CatalogBrand> CatalogBrands { get; set; }
        public virtual DbSet<CatalogItem> CatalogItems { get; set; }
        public virtual DbSet<CatalogItemsStock> CatalogItemsStocks { get; set; }
        public virtual DbSet<CatalogType> CatalogTypes { get; set; }
        public virtual DbSet<DiscountItem> DiscountItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CatalogBrand>()
                .Property(e => e.Brand)
                .IsUnicode(false);

            // The EF6 model called HasPrecision(19, 4) here. CatalogItem.Price already carries
            // [Column(TypeName = "money")], and EF Core applies precision facets to the
            // configured store type — which would emit the invalid "money(19,4)". money is
            // fixed at 19,4, so the store type alone reproduces the legacy column exactly.
            modelBuilder.Entity<CatalogItem>();

            modelBuilder.Entity<CatalogItemsStock>();

            modelBuilder.Entity<CatalogType>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<DiscountItem>();
        }
    }
}
