using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KiCData.Models
{
    public class InventoryItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id{ get; set; }
        public string? Name{ get; set; }
        public int? PriceInCents{ get; set; }
        public string? Type{ get; set; }
        public int? Stock{ get; set; }
        public string? ImgPath{ get; set; }
        public string? Description{ get; set; }
        
        [NotMapped]
        public int? PriceInDollars
        {
            get
            {
                if(this.PriceInCents is not null)
                {
                    return this.PriceInCents / 100;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}