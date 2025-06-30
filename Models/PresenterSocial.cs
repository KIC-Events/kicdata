using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KiCData.Models
{
  [Table("PresenterSocial")]
  public class PresenterSocial
  {
      public int Id { get; set; }

      public string Platform { get; set; } = default!;

      public string Handle { get; set; } = default!;

      public int PresenterId { get; set; }

      public Presenter Presenter { get; set; } = default!;
  }
}