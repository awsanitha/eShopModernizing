namespace eShopLegacyWebForms.Models
{
    public class CatalogItem
    {
        public const string DefaultPictureName = "dummy.png";

        public CatalogItem()
        {
            PictureFileName = DefaultPictureName;
        }
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        // decimal(18,2)
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^\d+(\.\d{0,2})*$", ErrorMessage = "The field Price must be a positive number with maximum two decimals.")]
        [System.ComponentModel.DataAnnotations.Range(0, 9999999999999999.99)]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        public decimal Price { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Picture name")]
        public string PictureFileName { get; set; }

        public string PictureUri { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Type")]
        public int CatalogTypeId { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Type")]
        public CatalogType CatalogType { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Brand")]
        public int CatalogBrandId { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Brand")]
        public CatalogBrand CatalogBrand { get; set; }

        // Quantity in stock
        [System.ComponentModel.DataAnnotations.Range(0, 10000000, ErrorMessage = "The field Stock must be between 0 and 10 million.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Stock")]
        public int AvailableStock { get; set; }

        // Available stock at which we should reorder
        [System.ComponentModel.DataAnnotations.Range(0, 10000000, ErrorMessage = "The field Stock must be between 0 and 10 million.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Restock")]
        public int RestockThreshold { get; set; }

        // Maximum number of units that can be in-stock at any time (due to physicial/logistical constraints in warehouses)
        [System.ComponentModel.DataAnnotations.Range(0, 10000000, ErrorMessage = "The field Stock must be between 0 and 10 million.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Max stock")]
        public int MaxStockThreshold { get; set; }

        /// <summary>
        /// True if item is on reorder
        /// </summary>
        public bool OnReorder { get; set; }
    }
}
