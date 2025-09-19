using KiCData.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace KiCData.Models
{
    [Table("Presentation")]
    public class Presentation
    {
        [Key]
        public int? Id { get; set; }

        [Required]
        [Display(Name = "The name of your class or presentation.")]
        public string? Name { get; set; }

        [Required]
        [Display(Name = "A short description of your class or presentation.")]
        public string? Description { get; set; }

        [Display(Name = "To what kink or kinks does your class pertain?")]
        public string? Type { get; set; }

        [JsonIgnore]
        public ICollection<Presenter> Presenters { get; set; } = new List<Presenter>();


        public int EventId { get; set; }
        public virtual Event Event { get; set; }

        public string? ImgPath { get; set; }
    }
}
