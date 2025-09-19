using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KiCData.Models
{
  [Table("Sponsor")]
  public class Sponsor
  {
    [Key]
    //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int? Id { get; set; }


    [Required]
    [Display(Name = @"The name we should use for you or your business in promotional materials.")]
    public string? PublicName { get; set; }

    [Display(Name = @"A tagline or short description of you or your business.")]
    public string? Tagline { get; set; }

    [Display(Name = @"A short bio about you or your business.")]
    public string? Bio { get; set; }

    public string? ImgPath { get; set; }

    [Display(Name = "The website URL for your business.")]
    public string? WebsiteUrl { get; set; }
    
    public int EventId { get; set; }
    
    public virtual Event? Event { get; set; }
    }
}