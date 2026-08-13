using eShopWCFService.Models;
using Microsoft.EntityFrameworkCore;

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
            modelBuilder.Entity<CatalogBrand>(entity =>
            {
                entity.Property(e => e.Brand)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CatalogItem>(entity =>
            {
                entity.ToTable("CatalogItems");
                entity.Property(e => e.Price)
                    .HasColumnType("decimal(19,4)");
            });

            modelBuilder.Entity<CatalogItemsStock>(entity =>
            {
                entity.ToTable("CatalogItemsStock");
                entity.HasKey(e => e.StockId);
                entity.Property(e => e.Date).HasColumnType("date");
            });

            modelBuilder.Entity<CatalogType>(entity =>
            {
                entity.Property(e => e.Type)
                    .IsRequired()
                    .IsUnicode(false);
            });

            modelBuilder.Entity<DiscountItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Start).HasColumnType("date");
                entity.Property(e => e.End).HasColumnType("date");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
