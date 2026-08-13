using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace eShopWCFService.Models
{
    [DataContract]
    public class DiscountItem
    {
        public DiscountItem()
        {
        }

        [DataMember]
        public double Size { get; set; }

        [Column(TypeName = "date")]
        [DataMember]
        public DateTime Start { get; set; }

        [Column(TypeName = "date")]
        [DataMember]
        public DateTime End { get; set; }

        [Key]
        [DataMember]
        public int Id { get; set; }
    }
}
