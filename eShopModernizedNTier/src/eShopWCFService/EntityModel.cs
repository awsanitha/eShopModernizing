using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Fallback connection string when not using DI (e.g. design-time tools)
                var connStr = CatalogConfiguration.ConnectionString;
                optionsBuilder.UseSqlServer(connStr);
            }
        }

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
        }
    }
}
