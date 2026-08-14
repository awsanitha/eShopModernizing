using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using eShopWCFService.Models;

namespace eShopWCFService
{
    public partial class EntityModel : DbContext
    {
        public EntityModel(DbContextOptions<EntityModel> options)
            : base(options)
        {
        }

        public virtual DbSet<CatalogBrand> CatalogBrands { get; set; } = null!;
        public virtual DbSet<CatalogItem> CatalogItems { get; set; } = null!;
        public virtual DbSet<CatalogItemsStock> CatalogItemsStocks { get; set; } = null!;
        public virtual DbSet<CatalogType> CatalogTypes { get; set; } = null!;
        public virtual DbSet<DiscountItem> DiscountItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
