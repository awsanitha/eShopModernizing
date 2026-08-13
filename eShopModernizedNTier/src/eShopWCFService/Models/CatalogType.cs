using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace eShopWCFService.Models
{
    [DataContract]
    public partial class CatalogType
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string? Type { get; set; }
    }
}
