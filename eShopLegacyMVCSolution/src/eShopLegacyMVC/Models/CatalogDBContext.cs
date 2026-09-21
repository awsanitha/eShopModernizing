using Microsoft.EntityFrameworkCore;

namespace eShopLegacyMVC.Models
{
    public class CatalogDBContext : DbContext
    {
        public CatalogDBContext(DbContextOptions<CatalogDBContext> options) : base(options)
        {
        }

        public DbSet<CatalogItem> CatalogItems { get; set; } = null!;
        public DbSet<CatalogBrand> CatalogBrands { get; set; } = null!;
        public DbSet<CatalogType> CatalogTypes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureCatalogType(modelBuilder);
            ConfigureCatalogBrand(modelBuilder);
            ConfigureCatalogItem(modelBuilder);
        }

        private void ConfigureCatalogType(ModelBuilder builder)
        {
            builder.Entity<CatalogType>().ToTable("CatalogType");
            builder.Entity<CatalogType>().HasKey(ct => ct.Id);
            builder.Entity<CatalogType>().Property(ct => ct.Type)
                .IsRequired()
                .HasMaxLength(100);
        }

        private void ConfigureCatalogBrand(ModelBuilder builder)
        {
            builder.Entity<CatalogBrand>().ToTable("CatalogBrand");
            builder.Entity<CatalogBrand>().HasKey(cb => cb.Id);
            builder.Entity<CatalogBrand>().Property(cb => cb.Brand)
                .IsRequired()
                .HasMaxLength(100);
        }

        private void ConfigureCatalogItem(ModelBuilder builder)
        {
            builder.Entity<CatalogItem>().ToTable("Catalog");
            builder.Entity<CatalogItem>().HasKey(ci => ci.Id);
            builder.Entity<CatalogItem>().Property(ci => ci.Id)
                .ValueGeneratedNever();
            builder.Entity<CatalogItem>().Property(ci => ci.Name)
                .IsRequired()
                .HasMaxLength(50);
            builder.Entity<CatalogItem>().Property(ci => ci.Price)
                .IsRequired();
            builder.Entity<CatalogItem>().Property(ci => ci.PictureFileName)
                .IsRequired();
            builder.Entity<CatalogItem>().Ignore(ci => ci.PictureUri);
            builder.Entity<CatalogItem>()
                .HasOne(ci => ci.CatalogBrand)
                .WithMany()
                .HasForeignKey(ci => ci.CatalogBrandId);
            builder.Entity<CatalogItem>()
                .HasOne(ci => ci.CatalogType)
                .WithMany()
                .HasForeignKey(ci => ci.CatalogTypeId);
        }
    }
}
