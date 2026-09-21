using System.ComponentModel.DataAnnotations;

namespace eShopLegacyMVC.Models
{
    public class CatalogItem
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        public string? Description { get; set; }

        [Required]
        [RegularExpression(@"^\$?\d+(\.(\d{2}))?$")]
        [Range(0, 1000000)]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        public string PictureFileName { get; set; } = "dummy.png";

        public string? PictureUri { get; set; }

        public int CatalogTypeId { get; set; }
        public CatalogType? CatalogType { get; set; }

        public int CatalogBrandId { get; set; }
        public CatalogBrand? CatalogBrand { get; set; }

        [Range(0, 10000000)]
        public int AvailableStock { get; set; }

        [Range(0, 10000000)]
        public int RestockThreshold { get; set; }

        [Range(0, 10000000)]
        public int MaxStockThreshold { get; set; }

        public bool OnReorder { get; set; }
    }
}
