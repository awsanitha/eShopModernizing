using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using eShopWCFService.Models;
using eShopWCFService.Models.Infrastructure;

namespace eShopWCFService
{
    public partial class EntityModel : DbContext
    {
        // Parameterless constructor — used when creating EntityModel without DI (e.g., tests, tools)
        // Connection is configured via OnConfiguring using CatalogConfiguration.ConnectionString
        public EntityModel() { }

        // DI constructor — preferred when injecting via IServiceCollection
        public EntityModel(DbContextOptions<EntityModel> options) : base(options) { }

        public virtual DbSet<CatalogBrand> CatalogBrands { get; set; }
        public virtual DbSet<CatalogItem> CatalogItems { get; set; }
        public virtual DbSet<CatalogItemsStock> CatalogItemsStocks { get; set; }
        public virtual DbSet<CatalogType> CatalogTypes { get; set; }
        public virtual DbSet<DiscountItem> DiscountItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Fallback for non-DI scenarios — reads connection string from environment or defaults
                optionsBuilder.UseSqlServer(CatalogConfiguration.ConnectionString);
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

            modelBuilder.Entity<CatalogItemsStock>(entity =>
            {
                entity.HasKey(e => e.StockId);
                entity.Property(e => e.Date).HasColumnType("date");
            });

            modelBuilder.Entity<CatalogType>()
                .Property(e => e.Type)
                .IsUnicode(false);

            modelBuilder.Entity<DiscountItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Start).HasColumnType("date");
                entity.Property(e => e.End).HasColumnType("date");
            });

            modelBuilder.Entity<CatalogItem>(entity =>
            {
                entity.Property(e => e.Price).HasColumnType("money");
            });
        }
    }
}
