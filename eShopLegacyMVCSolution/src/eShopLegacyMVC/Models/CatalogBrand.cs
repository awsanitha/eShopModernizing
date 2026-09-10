using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace eShopLegacyMVC.Models
{
    public class CatalogBrand
    {
        public int Id { get; set; }
        public string Brand { get; set; } = string.Empty;
    }
}
