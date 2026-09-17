using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using eShopWCFService.Models;
using eShopWCFService.Models.Infrastructure;

namespace eShopWCFService
{
    public partial class EntityModel : DbContext
    {
        public EntityModel()
        {
        }

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

            modelBuilder.Entity<CatalogItem>()
                .Property(e => e.Price)
                .HasPrecision(19, 4);

            modelBuilder.Entity<CatalogItemsStock>();

            modelBuilder.Entity<CatalogType>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<DiscountItem>();

            // Seed data (replaces EF6 CatalogDBInitializer)
            modelBuilder.Entity<CatalogType>().HasData(PreconfiguredData.GetPreconfiguredCatalogTypes());
            modelBuilder.Entity<CatalogBrand>().HasData(PreconfiguredData.GetPreconfiguredCatalogBrands());
            modelBuilder.Entity<CatalogItem>().HasData(PreconfiguredData.GetPreconfiguredCatalogItems());
            modelBuilder.Entity<CatalogItemsStock>().HasData(PreconfiguredData.GetPreconfiguredCatalogItemsStock());
            modelBuilder.Entity<DiscountItem>().HasData(PreconfiguredData.GetPreconfiguredDiscountItems());
        }
    }
}
